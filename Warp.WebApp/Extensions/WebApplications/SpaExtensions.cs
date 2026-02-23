using System.Globalization;
using System.Text.Json;
using Warp.WebApp.Helpers.Configuration;
using Warp.WebApp.Models.Options;

namespace Warp.WebApp.Extensions.WebApplications;

internal static class SpaExtensions
{
    internal static WebApplication MapSpa(this WebApplication app)
    {
        if (app.Environment.IsLocal())
        {
            app.MapWhen(ShouldProxyToSpa, spaApp =>
            {
                spaApp.UseSpa(spa =>
                {
                    var spaDevServerUrl = app.Configuration["Spa:ServerUrl"]!;

                    spa.Options.SourcePath = Path.Combine(app.Environment.ContentRootPath, "..", "Warp.ClientApp");
                    spa.UseProxyToSpaDevelopmentServer(spaDevServerUrl);
                });
            });
        }
        else
        { 
            app.MapWhen(ShouldServeSpa, spaApp =>
            {
                spaApp.Run(async ctx =>
                {
                    ctx.Response.ContentType = "text/html; charset=utf-8";
                    ctx.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";

                    await ctx.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
                });
            });
        }

        return app;


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


        bool ShouldServeSpa(HttpContext ctx)
        {
            var path = ctx.Request.Path.Value ?? string.Empty;

            // Skip API / health / backend-served resources
            if (_passthroughPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Let static files (with extensions) be handled by UseStaticFiles
            if (Path.HasExtension(path))
                return false;

            // Accept: text/html or root → SPA
            if (path == "/")
                return true;

            if (ctx.Request.Headers.TryGetValue("Accept", out var accept) && accept.ToString().Contains("text/html", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }


    internal static WebApplication MapSpaAnalytics(this WebApplication app)
    {
        app.MapGet("/analytics.js", async (HttpContext ctx, IConfiguration configuration, IWebHostEnvironment env) =>
        {
            var analyticsOptions = configuration.GetSection(nameof(AnalyticsOptions)).Get<AnalyticsOptions>() ?? new AnalyticsOptions();

            var analytics = string.Empty;
            if (!string.IsNullOrWhiteSpace(analyticsOptions.GoogleGTag))
                analytics += GoogleGTagScript.Replace("@Model.GTag", analyticsOptions.GoogleGTag);

            if (!string.IsNullOrWhiteSpace(analyticsOptions.YandexMetrikaNumber))
                analytics += YandexMetrikaScript.Replace("@Model.YandexMetrikaNumber", analyticsOptions.YandexMetrikaNumber);

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
            var config = new Dictionary<string, object?>
            {
                ["apiBaseUrl"] = "/api",
                ["environment"] = env.EnvironmentName,
                ["maxContentDeltaSize"] = configuration.GetValue<int>("Content:SizeLimits:MaxContentDeltaSize"),
                ["maxHtmlContentSize"] = configuration.GetValue<int>("Content:SizeLimits:MaxHtmlSize"),
                ["maxPlainTextContentSize"] = configuration.GetValue<int>("Content:SizeLimits:MaxPlainTextSize"),
                ["sentryDsn"] = configuration["Sentry:FrontendDsn"],
                ["feedbackEmail"] = configuration["ContactEmails:FeedbackEmail"],
                ["dataRequestEmail"] = configuration["ContactEmails:DataRequestEmail"]
            };

            var tracesSampleRate = ParseSampleRate(configuration, "Sentry:FrontendTracesSampleRate");
            if (tracesSampleRate.HasValue)
                config["sentryTracesSampleRate"] = tracesSampleRate.Value;

            var profilesSampleRate = ParseSampleRate(configuration, "Sentry:FrontendProfilesSampleRate");
            if (profilesSampleRate.HasValue)
                config["sentryProfilesSampleRate"] = profilesSampleRate.Value;

            var js = $"window.appConfig = {JsonSerializer.Serialize(config)};";

            ctx.Response.ContentType = "application/javascript";
            ctx.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
            await ctx.Response.WriteAsync(js);
        });

        return app;
    
    
        static double? ParseSampleRate(IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
                return null;

            return Math.Clamp(parsed, 0d, 1d);
        }
    }


    internal static WebApplication UseSpaStaticFiles(this WebApplication app)
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                var path = ctx.File.PhysicalPath ?? string.Empty;
                var name = Path.GetFileName(path);

                if (name.Equals("index.html", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".map", StringComparison.OrdinalIgnoreCase))
                {
                    ctx.Context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
                    return;
                }

                if (path.Contains(_assetsPathSegment, StringComparison.OrdinalIgnoreCase) || name.Contains('.'))
                    ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            }
        });

        return app;
    }


    private const string GoogleGTagScript = """
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
        """;

    private const string YandexMetrikaScript = """
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
        """;

    private static readonly string _assetsPathSegment = $"{Path.DirectorySeparatorChar}assets{Path.DirectorySeparatorChar}";
    private static readonly HashSet<string> _passthroughPrefixes = ["/api", "/health", "/config.js", "/analytics.js", "/robots.txt"];
    private static readonly HashSet<string> _spaAssetPrefixes = [ "/@vite", "/src", "/node_modules", "/assets", "/fonts", "/img", "/css", "/vendor" ];
}
