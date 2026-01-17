namespace Warp.WebApp.Helpers.Configuration;

public static class WebHostEnvironmentHelper
{
    public static bool IsDevelopmentOrLocal(this IWebHostEnvironment environment)
        => environment.IsDevelopment() || environment.IsLocal();


    public static bool IsLocal(this IWebHostEnvironment environment)
        => environment.IsEnvironment("Local");


    public static bool IsEndToEndTests(this IWebHostEnvironment environment)
        => environment.IsEnvironment("E2E") || environment.IsEnvironment("E2ELocal");
}