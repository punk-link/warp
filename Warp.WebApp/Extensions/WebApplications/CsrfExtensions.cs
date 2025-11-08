using Microsoft.AspNetCore.Antiforgery;

namespace Warp.WebApp.Extensions.WebApplications;

internal static class CsrfExtensions
{
    internal static WebApplication MapCsrf(this WebApplication app)
    {
        app.MapGet("/api/security/csrf", (IAntiforgery antiforgery, HttpContext ctx) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(ctx);
            if (!string.IsNullOrEmpty(tokens.RequestToken))
            {
                ctx.Response.Cookies.Append(
                    "XSRF-TOKEN",
                    tokens.RequestToken!,
                    new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Path = "/"
                    });
            }

            return Results.NoContent();
        });

        return app;
    }
}
