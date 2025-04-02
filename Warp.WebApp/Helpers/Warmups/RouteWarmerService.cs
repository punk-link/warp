using Microsoft.Extensions.Options;
using System.Diagnostics;
using Warp.WebApp.Attributes;
using Warp.WebApp.Constants;

namespace Warp.WebApp.Helpers.Warmups;

/// <summary>
/// Implementation of <see cref="IRouteWarmer"/> that uses an HttpClient to warm up routes
/// </summary>
public class RouteWarmerService : IRouteWarmer
{
    public RouteWarmerService(
        IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory, IOptions<RoutesWarmupOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _logger = loggerFactory.CreateLogger<RouteWarmerService>();
        _options = options.Value;
    }


    /// <inheritdoc />
    [TraceMethod]
    public async Task WarmUpRoutes(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting route warmup for {_options.Routes.Count} routes");
        
        var stopwatch = Stopwatch.StartNew();
        var client = _httpClientFactory.CreateClient(HttpClients.Warmup);

        int successCount = 0;
        foreach (var route in _options.Routes)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Route warmup canceled before completion");
                break;
            }

            try
            {
                var routeStopwatch = Stopwatch.StartNew();
                var requestUri = new Uri($"{_options.BaseUrl}{route}");
                
                _logger.LogDebug($"Warming route: {requestUri}");
                
                var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                routeStopwatch.Stop();
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug($"Successfully warmed route {route} in {routeStopwatch.ElapsedMilliseconds}ms");
                    successCount++;
                }
                else
                {
                    _logger.LogWarning($"Failed to warm route {route}: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error warming route {route}");
            }
        }

        stopwatch.Stop();
        _logger.LogInformation($"Route warming completed: {successCount}/{_options.Routes.Count} routes in {stopwatch.ElapsedMilliseconds}ms");
    }


    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RouteWarmerService> _logger;
    private readonly RoutesWarmupOptions _options;
}
