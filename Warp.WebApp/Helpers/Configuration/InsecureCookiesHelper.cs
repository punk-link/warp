namespace Warp.WebApp.Helpers.Configuration;

public static class InsecureCookiesHelper
{
    /// <summary>
    /// Indicates whether insecure cookies are allowed based on the environment.
    /// </summary>
    /// <param name="environment"></param>
    /// <returns></returns>
    /// <remarks>
    /// For end-to-end-tests environment we need to allow insecure cookies
    /// Webkit based browsers block cookies with Secure flag for end-to-end tests on localhost.
    /// </remarks>
    public static bool IsAllowed(IWebHostEnvironment environment)
        => environment.IsEndToEndTests() || environment.IsLocal();
}
