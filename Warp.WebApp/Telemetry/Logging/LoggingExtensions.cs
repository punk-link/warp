namespace Warp.WebApp.Telemetry.Logging;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        #if DEBUG
        builder.Logging.AddDebug();
        #endif

        builder.Logging.AddConsole();
        if (!string.IsNullOrWhiteSpace(builder.Configuration["SentryDsn"]))
            builder.Logging.AddSentry(o =>
            {
                o.Dsn = builder.Configuration["SentryDsn"];
                o.AttachStacktrace = true;
            });

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        return builder;
    }


    internal static ILogger<Program> GetStartUpLogger(this WebApplicationBuilder builder)
    {
        var logLevel = Enum.Parse<LogLevel>(builder.Configuration["Logging:LogLevel:Default"]!);
        using var loggerFactory = LoggerFactory.Create(loggerBuilder => loggerBuilder
            .SetMinimumLevel(logLevel)
            .AddConsole());

        return loggerFactory.CreateLogger<Program>();
    }
}
