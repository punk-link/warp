namespace Warp.WebApp.Extensions.WebApplications;

internal static class RobotsTxtExtensions
{
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
}
