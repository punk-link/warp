using Microsoft.AspNetCore.Antiforgery;
using Warp.WebApp.Helpers.Configuration;

namespace Warp.WebApp.Extensions.WebApplications;

internal static class CsrfExtensions
{
    internal static WebApplication MapCsrf(this WebApplication app)
    {
        var allowInsecureCookies = InsecureCookiesHelper.IsAllowed(app.Services.GetRequiredService<IWebHostEnvironment>());

        app.MapGet("/api/security/csrf", (IAntiforgery antiforgery, HttpContext ctx) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(ctx);
            if (!string.IsNullOrEmpty(tokens.RequestToken))
            {
                var secureFlag = !allowInsecureCookies && ctx.Request.IsHttps;

                ctx.Response.Cookies.Append(
                    "XSRF-TOKEN",
                    tokens.RequestToken!,
                    new CookieOptions
                    {
                        HttpOnly = false,
                        Secure = secureFlag,
                        SameSite = SameSiteMode.Strict,
                        Path = "/"
                    });
            }

            return Results.NoContent();
        });

        return app;
    }
}
