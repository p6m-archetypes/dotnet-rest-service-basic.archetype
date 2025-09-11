
using {{ PrefixName}}{{ SuffixName }}.Server.Grpc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;
using CorrelationId.DependencyInjection;
using CorrelationId;
using {{ PrefixName}}{{ SuffixName }}.Core;
using {{ PrefixName}}{{ SuffixName }}.Core.Services;
using {{ PrefixName}}{{ SuffixName }}.Server.Services;
using {{ PrefixName}}{{ SuffixName }}.Server.Interceptors;
using {{ PrefixName}}{{ SuffixName }}.Server.HealthChecks;
using System.Text.Json;


namespace {{ PrefixName}}{{ SuffixName }}.Server;

public class Startup
{
    public Startup(IConfigurationRoot configuration)
    {
        Configuration = configuration;
    }
    public IConfigurationRoot Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Determine environment modes
        bool isEphemeral = Configuration["ASPNETCORE_ENVIRONMENT"] == "Ephemeral" || 
                          Configuration["SPRING_PROFILES_ACTIVE"]?.Contains("ephemeral") == true;
        
        // Add services to the container.
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<MetricsInterceptor>();
            options.Interceptors.Add<GlobalExceptionInterceptor>();
            
            // Skip authorization for ephemeral environments
            if (!isEphemeral)
            {
                options.Interceptors.Add<AuthorizationInterceptor>();
            }
        });
        services.AddGrpcReflection();
        services.AddControllers();

        // Add correlation ID support
        services.AddHttpContextAccessor();
        services.AddDefaultCorrelationId();


        // Configure authorization (authentication happens at API Gateway)
        // Configure authentication and authorization services
        services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
        
        // Skip authorization services for ephemeral environments
        if (!isEphemeral)
        {
            services.AddScoped<IApiGatewayJwtValidator, ApiGatewayJwtValidator>();
            services.AddScoped<AuthorizationInterceptor>();
        }

        // Add authorization for potential future policy-based authorization
        services.AddAuthorization();

        // Add metrics service
        services.AddSingleton<MetricsService>();
        services.AddScoped<MetricsInterceptor>();

        // Add graceful shutdown service
        services.AddHostedService<Services.GracefulShutdownService>();

        services.AddScoped<{{ PrefixName}}{{ SuffixName }}Core>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<GlobalExceptionInterceptor>();
        
        
        // Register health check services
        services.AddScoped<ServiceHealthCheck>();
        
        // Enhanced health checks with dependency validation
        services.AddHealthChecks()
            .AddCheck<ServiceHealthCheck>(
                "services",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "services"])
            .AddCheck("self", 
                () => HealthCheckResult.Healthy("Application is running"),
                tags: ["live"]);

        // Enhanced OpenTelemetry configuration with custom metrics
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                Configuration["Application:Name"] ?? "example-service",
                Configuration["Application:Version"] ?? "1.0.0",
                serviceInstanceId: Environment.MachineName
            ))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddHttpClientInstrumentation()
                    .AddMeter("{{ PrefixName}}{{ SuffixName }}"); // Add our custom metrics
                
                // Export to multiple endpoints
                if (Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] != null)
                {
                    metrics.AddOtlpExporter();
                }
                metrics.AddPrometheusExporter();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.FilterHttpRequestMessage = (httpRequestMessage) =>
                        {
                            // Don't trace health check requests to reduce noise
                            return !httpRequestMessage.RequestUri?.PathAndQuery.Contains("/health") == true;
                        };
                    })
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = (httpContext) =>
                        {
                            // Don't trace health check and metrics requests
                            var path = httpContext.Request.Path.Value;
                            return !path?.Contains("/health") == true && !path?.Contains("/metrics") == true;
                        };
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                    })
                    .AddGrpcCoreInstrumentation()
                    .SetSampler(new TraceIdRatioBasedSampler(
                        double.Parse(Configuration["OTEL_TRACES_SAMPLER_ARG"] ?? "1.0")));
                
                // Export traces
                if (Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] != null)
                {
                    tracing.AddOtlpExporter();
                }
                
                // Console exporter not needed for production services
            });
    }
        
        
    public void Configure(WebApplication app)
    {
        // Determine environment modes (same as in ConfigureServices)
        bool isEphemeral = Configuration["ASPNETCORE_ENVIRONMENT"] == "Ephemeral" || 
                          Configuration["SPRING_PROFILES_ACTIVE"]?.Contains("ephemeral") == true;
        bool enableMigrations = bool.Parse(Configuration["Database:EnableMigrations"] ?? "true");
        bool dropCreateDatabase = bool.Parse(Configuration["Database:DropCreateDatabase"] ?? "false");
        
        // Handle database setup based on environment
        if (enableMigrations)
        {
            using (var scope = app.Services.CreateScope())
            {
                var servicesProvider = scope.ServiceProvider;
                var logger = servicesProvider.GetRequiredService<ILogger<Startup>>();
            }
        }
        else
        {
            var logger = app.Services.GetRequiredService<ILogger<Startup>>();
            logger.LogWarning("Database setup skipped - enableMigrations: {EnableMigrations}", enableMigrations);
        }

        // Configure the HTTP request pipeline.
        // Add correlation ID middleware early in the pipeline
        app.UseCorrelationId();
        
        // Note: Authentication happens at API Gateway, authorization in gRPC interceptor
        
        app.MapGrpcReflectionService().AllowAnonymous();
        app.MapGrpcService<{{ PrefixName}}{{ SuffixName }}GrpcImpl>();
        app.MapControllers();
        app.MapGet("/", () => "{{ PrefixName}}{{ SuffixName }}");

        app.MapPrometheusScrapingEndpoint("/metrics");
        
        // Comprehensive health check endpoint
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description ?? "No description",
                        duration = e.Value.Duration.ToString(),
                        tags = e.Value.Tags
                    }),
                    totalDuration = report.TotalDuration.ToString()
                });
                await context.Response.WriteAsync(result);
            }
        });
        
        // Kubernetes liveness probe - basic application health
        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });
        
        // Kubernetes readiness probe - dependencies health
        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        // Ephemeral mode logging - persistence layer has been removed
        if (isEphemeral)
        {
            var logger = app.Services.GetRequiredService<ILogger<Startup>>();
            logger.LogInformation("Running in ephemeral mode (persistence layer removed)");
        }
    }
}

