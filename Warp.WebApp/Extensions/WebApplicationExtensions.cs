using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Warp.WebApp.Extensions.WebApplications;
using Warp.WebApp.Helpers.Configuration;
using Warp.WebApp.Middlewares;

namespace Warp.WebApp.Extensions;

internal static class WebApplicationExtensions
{
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

        app.UseMiddleware<TraceContextMiddleware>();
        app.UseMiddleware<ApiExceptionHandlerMiddleware>();
        app.UseMiddleware<TraceMethodMiddleware>();
        app.UseMiddleware<CancellationExceptionHandlerMiddleware>();
        app.UseHealthChecks("/health");

        app.UseHttpsRedirection();
        app.UseSpaStaticFiles();
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
}
