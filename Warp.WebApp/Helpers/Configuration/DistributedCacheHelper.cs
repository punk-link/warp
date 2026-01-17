using StackExchange.Redis;
using System.Globalization;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Helpers.Configuration;

public static class DistributedCacheHelper
{
    public static IConnectionMultiplexer GetConnectionMultiplexer(ILogger logger, IConfiguration configuration)
    {
        var host = configuration["Redis:Host"]!;
        if (string.IsNullOrWhiteSpace(host))
            logger.LogRedisHostIsNotSpecified();

        if (!int.TryParse(configuration["Redis:Port"]!, NumberStyles.Integer, CultureInfo.InvariantCulture, out var port))
            logger.LogRedisPortIsNotSpecified();

        var abortOnConnectFail = configuration.GetValue("Redis:AbortOnConnectFail", defaultValue: true);
        var connectRetry = configuration.GetValue("Redis:ConnectRetry", defaultValue: 3);
        var connectTimeout = configuration.GetValue("Redis:ConnectTimeout", defaultValue: 5000);

        var options = new ConfigurationOptions
        {
            AbortOnConnectFail = abortOnConnectFail,
            ConnectRetry = connectRetry,
            ConnectTimeout = connectTimeout,
            EndPoints =
            {
                { host, port }
            },
            ReconnectRetryPolicy = new ExponentialRetry(5_000, 20_000),
            Ssl = false
        };

        try
        {
            return ConnectionMultiplexer.Connect(options);
        }
        catch (Exception ex)
        {
            logger.LogRedisConnectionException(ex.Message);
            throw;
        }
    }


    public static string GetConnectionString(ILogger logger, IConfiguration configuration)
    {
        var host = configuration["Redis:Host"]!;
        if (string.IsNullOrWhiteSpace(host))
            logger.LogRedisHostIsNotSpecified();

        if (!int.TryParse(configuration["Redis:Port"]!, NumberStyles.Integer, CultureInfo.InvariantCulture, out var port))
            logger.LogRedisPortIsNotSpecified();

        return $"{host}:{port},abortConnect=true,ssl=false";
    }
}