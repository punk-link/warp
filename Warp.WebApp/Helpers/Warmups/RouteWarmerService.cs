using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using Warp.WebApp.Attributes;

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
        if (_options.Routes == null || !_options.Routes.Any())
        {
            _logger.LogInformation("No routes configured for warming");
            return;
        }

        _logger.LogInformation("Starting route warmup for {Count} routes", _options.Routes.Count);
        
        var stopwatch = Stopwatch.StartNew();
        var client = _httpClientFactory.CreateClient("warmup");

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
                
                _logger.LogDebug("Warming route: {Route}", requestUri);
                
                var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                routeStopwatch.Stop();
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("Successfully warmed route {Route} in {ElapsedMilliseconds}ms", route, routeStopwatch.ElapsedMilliseconds);
                    successCount++;
                }
                else
                {
                    _logger.LogWarning("Failed to warm route {Route}: {StatusCode}", route, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error warming route {Route}", route);
            }
        }


        stopwatch.Stop();
        _logger.LogInformation("Route warming completed: {SuccessCount}/{TotalCount} routes in {ElapsedMilliseconds}ms", 
            successCount, _options.Routes.Count, stopwatch.ElapsedMilliseconds);
    }


    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RouteWarmerService> _logger;
    private readonly RoutesWarmupOptions _options;
}
