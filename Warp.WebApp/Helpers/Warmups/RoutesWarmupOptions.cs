namespace Warp.WebApp.Helpers.Warmups;

/// <summary>
/// Options for configuring route warmup
/// </summary>
public class RoutesWarmupOptions
{
    /// <summary>
    /// Base URL for the application (defaults to http://localhost)
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost";

    /// <summary>
    /// Collection of routes to warm up
    /// </summary>
    public List<string> Routes { get; set; } = new List<string>
    {
        "/",
        "/deleted",
        "/error",
        "/health",
        "/api/entries"
    };
}
