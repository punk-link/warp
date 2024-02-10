namespace Warp.WebApp.Helpers.Configuration;

public static class WebHostEnvironmentHelper
{
    public static bool IsDevelopmentOrLocal(this IWebHostEnvironment environment)
        => environment.IsDevelopment() || environment.IsEnvironment("Local");
}