namespace Warp.WebApp.Helpers.Warmups;

/// <summary>
/// Service responsible for warming up application routes via HTTP requests
/// </summary>
public interface IRouteWarmer
{
    /// <summary>
    /// Warms up the configured routes by making HTTP requests to them
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task WarmUpRoutes(CancellationToken cancellationToken);
}
