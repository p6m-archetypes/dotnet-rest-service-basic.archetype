using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using OpenTelemetry.Logs;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Enrichers.CorrelationId;
using {{ PrefixName}}{{ SuffixName }}.Server.Services;

namespace {{ PrefixName}}{{ SuffixName }}.Server;

public class {{ PrefixName}}{{ SuffixName }}Server
{
    private string[] args = [];
    private WebApplication? app;

    public {{ PrefixName}}{{ SuffixName }}Server Start()
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure graceful shutdown
        builder.Host.ConfigureHostOptions(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(30);
        });
        
        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            // Check if structured logging is requested
            var useStructuredLogging = Environment.GetEnvironmentVariable("LOGGING_STRUCTURED") == "true";
            
            // Configure base settings from configuration (minimum levels, enrichers, etc.)
            var config = builder.Configuration;
            
            // Apply minimum levels
            loggerConfig.MinimumLevel.Information();
            if (config.GetSection("Serilog:MinimumLevel:Override").Exists())
            {
                foreach (var overrideConfig in config.GetSection("Serilog:MinimumLevel:Override").GetChildren())
                {
                    var levelValue = overrideConfig.Value;
                    if (Enum.TryParse<Serilog.Events.LogEventLevel>(levelValue, out var level))
                    {
                        loggerConfig.MinimumLevel.Override(overrideConfig.Key, level);
                    }
                }
            }
            
            // Apply enrichers
            loggerConfig
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithCorrelationId()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("Application", config["Application:Name"] ?? "{{ prefix-name }}-{{ suffix-name }}")
                .Enrich.WithProperty("Version", config["Application:Version"] ?? "1.0.0")
                .Enrich.WithProperty("Environment", config["Application:Environment"] ?? "Production");
            
            // Configure output based on LOGGING_STRUCTURED environment variable
            if (useStructuredLogging)
            {
                // Use structured JSON logging
                loggerConfig.WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter());
            }
            else
            {
                // Use line-based logging with a readable format
                loggerConfig.WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
                );
            }
        });
        
        builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter());

        var startup = new Startup(builder.Configuration);
        startup.ConfigureServices(builder.Services);
        app = builder.Build();
        
        // Ephemeral mode check removed - persistence layer has been removed
        
        startup.Configure(app);
        app.Start();

        return this;
    }

    public {{ PrefixName}}{{ SuffixName }}Server Stop()
    {
        if (app != null)
        {
            // Persistence layer has been removed
            
            app.StopAsync().GetAwaiter().GetResult();
        }
        return this;
    }

    public {{ PrefixName}}{{ SuffixName }}Server WithArguments(string[] args)
    {
        this.args = args;
        return this;
    }
    
    public {{ PrefixName}}{{ SuffixName }}Server WithEphemeral()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Ephemeral");
        Environment.SetEnvironmentVariable("SPRING_PROFILES_ACTIVE", "ephemeral");
        return this;
    }

     public {{ PrefixName}}{{ SuffixName }}Server WithRandomPorts()
    {
        // Use fixed test ports for integration tests to avoid port discovery issues
        Environment.SetEnvironmentVariable("GRPC_PORT", "5040");
        Environment.SetEnvironmentVariable("HTTP_PORT", "5041");
        return this;
    }

    public string? getGrpcUrl()
    {
        if (app == null) return null;
        
        try
        {
            var serverAddresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
            // Look for the gRPC endpoint (HTTP/2) - should be the one NOT on management port
            var grpcAddress = serverAddresses?.Addresses.FirstOrDefault(addr => !addr.Contains("5031"));
            
            if (grpcAddress != null)
            {
                // Replace 0.0.0.0 with localhost for client connections
                return grpcAddress.Replace("0.0.0.0", "localhost");
            }
            
            // Fallback to configured gRPC port
            return app.Configuration["Kestrel:Endpoints:Grpc:Url"]?.Replace("0.0.0.0", "localhost") 
                   ?? "http://localhost:5030";
        }
        catch
        {
            return "http://localhost:5030";
        }
    }
    
    public static async Task Main(string[] args)
    {
        var server = new {{ PrefixName}}{{ SuffixName }}Server()
            .WithArguments(args);

        // Parse command-line arguments for special modes
        if (args.Contains("--ephemeral"))
        {
            server.WithEphemeral();
        }
        

        server.Start();

        // Simulate waiting for shutdown signal or some other condition
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            // Register an event to stop the app when Ctrl+C or another shutdown signal is received
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true; // Prevent the app from terminating immediately
                cancellationTokenSource.Cancel(); // Trigger the stop signal
            };

            try
            {
                // Wait indefinitely (or for a shutdown signal) by awaiting on the task
                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // The delay task is canceled when a shutdown signal is received
            }
        }

        // Gracefully stop the application
        server.Stop();
    }
}
