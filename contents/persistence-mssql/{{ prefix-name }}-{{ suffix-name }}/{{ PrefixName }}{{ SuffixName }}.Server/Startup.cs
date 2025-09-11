using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using {{ PrefixName }}{{ SuffixName }}.Core;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Context;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;
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


        // Configure ephemeral database service if needed
        if (isEphemeral)
        {
            services.Configure<EphemeralDatabaseOptions>(Configuration.GetSection(EphemeralDatabaseOptions.SectionName));
            services.AddSingleton<EphemeralDatabaseService>();
            services.AddHostedService<EphemeralDatabaseHostedService>();
        }
        
        // Configure database connection
        
        // Configure database with optimized connection pooling
        if (isEphemeral)
        {
            // Use Testcontainers PostgreSQL for ephemeral environment
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var ephemeralDbService = serviceProvider.GetRequiredService<EphemeralDatabaseService>();
                var connectionString = ephemeralDbService.GetConnectionString();
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Ephemeral database connection string is not available. Ensure the EphemeralDatabaseService has been started.");
                }
                
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                    
                    npgsqlOptions.CommandTimeout(
                        int.Parse(Configuration["Database:CommandTimeout"] ?? "30"));
                });
                
                // Configure connection pooling and logging for ephemeral mode
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging(Configuration["ASPNETCORE_ENVIRONMENT"] == "Development");
                options.EnableServiceProviderCaching();
            });
        }
        else
        {
            // Use regular PostgreSQL connection for production
            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString("DefaultConnection");
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                    
                    npgsqlOptions.CommandTimeout(
                        int.Parse(Configuration["Database:CommandTimeout"] ?? "30"));
                });

                // Enable sensitive data logging only in development
                if (Configuration["ASPNETCORE_ENVIRONMENT"] == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }

                // Configure connection pooling
                options.EnableServiceProviderCaching();
            });
            services.AddScoped<I{{ PrefixName }}Repository, {{ PrefixName }}Repository>();
        }        
        
        // Register health check services
        services.AddScoped<DatabaseHealthCheck>();
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

        // Only add database health check if explicitly enabled
        if (Configuration.GetValue<bool>("HealthChecks:Database:Enabled", false) || isEphemeral)
        {
            healthChecksBuilder.AddCheck<DatabaseHealthCheck>(
                "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db"]);
        }

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
        
        // Handle database setup based on environment
        if (enableMigrations && isEphemeral)
        {
            using (var scope = app.Services.CreateScope())
            {
                var servicesProvider = scope.ServiceProvider;
                var context = servicesProvider.GetRequiredService<AppDbContext>();
                var logger = servicesProvider.GetRequiredService<ILogger<Startup>>();
                
                if (isEphemeral)
                {
                    // For ephemeral mode, ensure clean database state
                    logger.LogInformation("Setting up ephemeral database schema...");
                    
                    try
                    {
                        if (dropCreateDatabase)
                        {
                            context.Database.EnsureDeleted();
                        }
                        
                        var created = context.Database.EnsureCreated();
                        logger.LogInformation("Database schema created successfully");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error during database schema creation");
                        throw;
                    }
                }
                else
                {
                    // For production/development, use migrations
                    logger.LogInformation("Running database migrations");
                    context.Database.Migrate();
                    logger.LogInformation("Database migrations completed");
                }
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

        // Display ephemeral database connection info after server is ready
        if (isEphemeral)
        {
            var appLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            appLifetime.ApplicationStarted.Register(() =>
            {
                var ephemeralDbService = app.Services.GetRequiredService<EphemeralDatabaseService>();
                ephemeralDbService.DisplayConnectionInfo();
            });
        }
    }
}
