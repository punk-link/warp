using StackExchange.Redis;
using System.Globalization;
using Warp.WebApp.Extensions.Logging;

namespace Warp.WebApp.Helpers.Configuration;

public static class DistributedCacheHelper
{
    public static IConnectionMultiplexer GetConnectionMultiplexer(ILogger logger, IConfiguration configuration)
    {
        var host = configuration["Redis:Host"]!;
        if (string.IsNullOrWhiteSpace(host))
            logger.LogRedisHostUnspecified();

        if (!int.TryParse(configuration["Redis:Port"]!, NumberStyles.Integer, CultureInfo.InvariantCulture, out var port))
            logger.LogRedisPortUnspecified();

        var options = new ConfigurationOptions
        {
            AbortOnConnectFail = true,
            EndPoints =
            {
                { host, port }
            },
            ReconnectRetryPolicy = new ExponentialRetry(5000),
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
}