using Warp.WebApp.Telemetry.Metrics;

namespace Warp.WebApp.Middlewares;

public class RobotsMiddleware
{
    public RobotsMiddleware(RequestDelegate next, IHostEnvironment environment)
    {
        _environment = environment;
        _next = next;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.Equals("/robots.txt"))
        {
            await _next(context);
            return;
        }

        var content = await GetFileContent(_environment.ContentRootPath);

        ApplicationMetrics.RobotsMiddlewareCounter.Add(1);

        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(content);
    }


    private static async ValueTask<string> GetFileContent(string contentRootPath)
    {
        var filePath = Path.Combine(contentRootPath, "robots.txt");
        return await File.ReadAllTextAsync(filePath);
    }


    private readonly IHostEnvironment _environment;
    private readonly RequestDelegate _next;
}