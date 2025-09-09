using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Text.Json;
using Warp.WebApp.Helpers.Configuration;
using Warp.WebApp.Middlewares;
using Warp.WebApp.Models.Options;

namespace Warp.WebApp.Extensions;

internal static class WebApplicationExtensions
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


    internal static WebApplication MapRobotsTxt(this WebApplication app)
    {
        app.MapGet("/robots.txt", async (IHostEnvironment env, HttpContext ctx) =>
        {
            var filePath = Path.Combine(env.ContentRootPath, "robots.txt");
            if (!File.Exists(filePath))
            {
                ctx.Response.StatusCode = 404;
                return;
            }

            var content = await File.ReadAllTextAsync(filePath);
            ctx.Response.ContentType = "text/plain";
            await ctx.Response.WriteAsync(content);
        });

        return app;
    }


    internal static WebApplication MapSpa(this WebApplication app)
    {
        if (app.Environment.IsLocal())
        {
            app.MapWhen(ctx => ShouldProxyToSpa(ctx), spaApp =>
            {
                spaApp.UseSpa(spa =>
                {
                    var spaDevServerUrl = app.Configuration["Spa:ServerUrl"]!;

                    spa.Options.SourcePath = Path.Combine(app.Environment.ContentRootPath, "..", "Warp.ClientApp");
                    spa.UseProxyToSpaDevelopmentServer(spaDevServerUrl);
                });
            });

            bool ShouldProxyToSpa(HttpContext ctx)
            {
                var path = ctx.Request.Path.Value ?? string.Empty;

                // Skip API / health / backend-served resources
                if (_passthroughPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                    return false;

                // Always proxy known Vite runtime/module/asset paths
                if (_spaAssetPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                    return true;

                // If request Accept header asks for HTML (navigation / client route), proxy.
                if (ctx.Request.Headers.TryGetValue("Accept", out var accept) && accept.ToString().Contains("text/html", StringComparison.OrdinalIgnoreCase))
                    return true;

                // Root path or no extension path (e.g., /preview/123) -> SPA
                if (path == "/" || (!Path.HasExtension(path)))
                    return true;

                return false;
            }
        }

        return app;
    }


    internal static WebApplication MapSpaAnalytics(this WebApplication app)
    { 
        app.MapGet("/analytics.js", async (HttpContext ctx, IConfiguration configuration, IWebHostEnvironment env) => 
        {
            var analyticsOptions = configuration.GetSection(nameof(AnalyticsOptions)).Get<AnalyticsOptions>() ?? new AnalyticsOptions();

            var analytics = string.Empty;

            if (!string.IsNullOrWhiteSpace(analyticsOptions.GoogleGTag))
            {
                analytics += """
                (function() {
                    const script = document.createElement('script');
                    script.async = true;
                    script.src = `https://www.googletagmanager.com/gtag/js?id=$@Model.GTag`;
                    document.head.appendChild(script);

                    window.dataLayer = window.dataLayer || [];
                    function gtag() { 
                        dataLayer.push(arguments); 
                    }

                    gtag('js', new Date());
                    gtag('config', '@Model.GTag');
                })();
                """.Replace("@Model.GTag", analyticsOptions.GoogleGTag);
            }

            if (!string.IsNullOrWhiteSpace(analyticsOptions.YandexMetrikaNumber))
            {
                analytics += """
                (function(m,e,t,r,i,k,a){m[i]=m[i]||function(){(m[i].a=m[i].a||[]).push(arguments)};
                m[i].l=1*new Date();
                for (var j = 0; j < document.scripts.length; j++) {if (document.scripts[j].src === r) { return; }}
                k=e.createElement(t),a=e.getElementsByTagName(t)[0],k.async=1,k.src=r,a.parentNode.insertBefore(k,a)})
                (window, document, "script", "https://mc.yandex.ru/metrika/tag.js", "ym");

                ym(@Model.YandexMetrikaNumber, "init", {
                    clickmap:true,
                    trackLinks:true,
                    accurateTrackBounce:true
                });
                """.Replace("@Model.YandexMetrikaNumber", analyticsOptions.YandexMetrikaNumber);
            }

            ctx.Response.ContentType = "application/javascript";
            ctx.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
            await ctx.Response.WriteAsync(analytics);
        });

        return app;
    }


    internal static WebApplication MapSpaConfigs(this WebApplication app)
    { 
        app.MapGet("/config.js", async (HttpContext ctx, IConfiguration configuration, IWebHostEnvironment env) =>
        {
            var config = new
            {
                apiBaseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}/api",
                environment = env.EnvironmentName,
                sentryDsn = configuration["Sentry:FrontendDsn"]
            };

            var js = $"window.appConfig = {JsonSerializer.Serialize(config)};";

            ctx.Response.ContentType = "application/javascript";
            ctx.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
            await ctx.Response.WriteAsync(js);
        });

        return app;
    }


    internal static WebApplication ConfigureWebApp(this WebApplication app)
    {
        var supportedCultures = new[] { new CultureInfo("en-US") };
        app.UseRequestLocalization(new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("en-US"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures
        });

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
        });

        if (!app.Environment.IsDevelopmentOrLocal())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseMiddleware<ApiExceptionHandlerMiddleware>();
        app.UseMiddleware<TraceMethodMiddleware>();
        app.UseMiddleware<CancellationExceptionHandlerMiddleware>();
        app.UseHealthChecks("/health");

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCookiePolicy();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseResponseCompression();
        app.UseResponseCaching();
        app.UseOutputCache();

        app.MapControllers();

        app.MapRobotsTxt()
            .MapCsrf()
            .MapSpaConfigs()
            .MapSpaAnalytics()
            .MapSpa();

        return app;
    }


    private static readonly HashSet<string> _passthroughPrefixes = [ "/api", "/health", "/config.js", "/analytics.js", "/robots.txt" ];
    private static readonly HashSet<string> _spaAssetPrefixes = [ "/@vite", "/src", "/node_modules", "/assets" ];
}
