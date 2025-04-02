namespace Warp.WebApp.Helpers.Warmups;

/// <summary>
/// Service responsible for warming up registered services in the dependency injection container
/// </summary>
public interface IServiceWarmer
{
    /// <summary>
    /// Warms up the services registered in the dependency injection container
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task WarmUpServices(CancellationToken cancellationToken);
}
