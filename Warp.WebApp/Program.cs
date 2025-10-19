using Warp.WebApp.Extensions;
using Warp.WebApp.Telemetry;
using Warp.WebApp.Telemetry.Logging;


internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseDefaultServiceProvider((context, options) =>
        {
            options.ValidateScopes = true;
            options.ValidateOnBuild = true;
        });

        var startupLogger = builder.GetStartUpLogger();

        try
        {
            await builder.AddConfiguration(startupLogger);

            builder.AddLogging()
                .AddTelemetry();

            builder.Services.AddServices(builder, startupLogger);

            builder.Build()
                .ConfigureWebApp()
                .Run();
        }
        catch (Exception ex)
        {
            startupLogger.LogCritical(ex, ex.Message);
            throw;
        }

        return;
    }
}