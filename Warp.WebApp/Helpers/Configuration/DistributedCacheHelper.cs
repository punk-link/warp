using System.Globalization;
using System.Net;
using StackExchange.Redis;
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

        var endpoints = new EndPointCollection(new List<EndPoint>
        {
            new DnsEndPoint(host, port)
        });

        var options = new ConfigurationOptions
        {
            AbortOnConnectFail = true,
            EndPoints = endpoints,
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