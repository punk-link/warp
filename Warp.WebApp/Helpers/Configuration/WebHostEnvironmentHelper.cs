namespace Warp.WebApp.Helpers.Configuration;

public static class WebHostEnvironmentHelper
{
    public static bool IsDevelopmentOrLocal(this IWebHostEnvironment environment)
        => environment.IsDevelopment() || environment.IsLocal();


    public static bool IsLocal(this IWebHostEnvironment environment)
        => environment.IsEnvironment("Local");
}