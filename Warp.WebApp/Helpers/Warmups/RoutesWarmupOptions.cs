namespace Warp.WebApp.Helpers.Warmups;

/// <summary>
/// Options for configuring route warmup
/// </summary>
public class RoutesWarmupOptions
{
    /// <summary>
    /// Base URL for the application (defaults to http://localhost:8080)
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:8080";

    /// <summary>
    /// Collection of routes to warm up
    /// </summary>
    public List<string> Routes { get; set; } = [];
}
