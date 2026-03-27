namespace Warp.WebApp.Extensions.WebApplications;

internal static class SitemapExtensions
{
    internal static WebApplication MapSitemap(this WebApplication app)
    {
        app.MapGet("/sitemap.xml", async (IHostEnvironment env, HttpContext ctx) =>
        {
            var filePath = Path.Combine(env.ContentRootPath, "sitemap.xml");
            if (!File.Exists(filePath))
            {
                ctx.Response.StatusCode = 404;
                return;
            }

            var content = await File.ReadAllTextAsync(filePath);
            ctx.Response.ContentType = "application/xml";
            await ctx.Response.WriteAsync(content);
        });

        return app;
    }
}
