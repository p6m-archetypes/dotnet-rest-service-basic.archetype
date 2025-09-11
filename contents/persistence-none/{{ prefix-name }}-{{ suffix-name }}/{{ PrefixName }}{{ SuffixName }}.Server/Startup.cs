using System.Text.Json;
using {{ PrefixName }}{{ SuffixName }}.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using CorrelationId.DependencyInjection;
using CorrelationId;
using {{ PrefixName }}{{ SuffixName }}.Core.Services;
using {{ PrefixName }}{{ SuffixName }}.Server.Services;
using {{ PrefixName }}{{ SuffixName }}.Server.HealthChecks;
using {{ PrefixName }}{{ SuffixName }}.Server.Middleware;


namespace {{ PrefixName }}{{ SuffixName }}.Server;

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
        services.AddControllers()
            .ConfigureApplicationPartManager(manager =>
            {
                // Explicitly ensure our controllers assembly is included
                manager.ApplicationParts.Add(new Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart(typeof(Controllers.AuthController).Assembly));
            });
        
        // Add Swagger/OpenAPI support
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "{{ PrefixName }}{{ SuffixName }} API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Add correlation ID support
        services.AddHttpContextAccessor();
        services.AddDefaultCorrelationId();


        // Configure JWT authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Authentication:Jwt:Issuer"] ?? "{{ PrefixName }}{{ SuffixName }}",
                    ValidAudience = Configuration["Authentication:Jwt:Audience"] ?? "{{ PrefixName }}{{ SuffixName }}API",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:Jwt:SecretKey"] ?? "ThisIsAVerySecretKeyForDevelopmentOnly123456789"))
                };
            });

        // Configure authentication and authorization services
        services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
        

        // Add authorization for potential future policy-based authorization
        services.AddAuthorization();

        // Add metrics service
        services.AddSingleton<MetricsService>();

        // Add graceful shutdown service
        services.AddHostedService<Services.GracefulShutdownService>();

        services.AddScoped<{{ PrefixName }}{{ SuffixName }}Core>();
        services.AddScoped<IValidationService, ValidationService>();
        
        
        // Register health check services
        services.AddScoped<ServiceHealthCheck>();
        
        // Enhanced health checks with dependency validation
        var healthChecksBuilder = services.AddHealthChecks()
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
                Configuration["Application:Name"] ?? "project-prefix-project-suffix",
                Configuration["Application:Version"] ?? "1.0.0",
                serviceInstanceId: Environment.MachineName
            ))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddHttpClientInstrumentation()
                    .AddMeter("{{ PrefixName }}{{ SuffixName }}"); // Add our custom metrics
                
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
        bool enableMigrations = bool.Parse(Configuration["Database:EnableMigrations"] ?? "false");
        bool dropCreateDatabase = bool.Parse(Configuration["Database:DropCreateDatabase"] ?? "false");
        
        var logger = app.Services.GetRequiredService<ILogger<Startup>>();

        // Configure the HTTP request pipeline.
        // Add correlation ID middleware early in the pipeline
        app.UseCorrelationId();
        
        // Add global exception handling
        app.UseMiddleware<GlobalExceptionMiddleware>();
        
        // Add metrics middleware
        app.UseMiddleware<MetricsMiddleware>();
        
        // Add Swagger UI (only in development/ephemeral environments)
        if (app.Environment.IsDevelopment() || isEphemeral)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => 
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "{{ PrefixName }}{{ SuffixName }} API V1");
                c.RoutePrefix = "swagger";
            });
        }
        
        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        app.MapGet("/", () => "{{ PrefixName }}{{ SuffixName }} REST API Service");

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
    }
}
