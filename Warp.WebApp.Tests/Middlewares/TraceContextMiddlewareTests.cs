using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Warp.WebApp.Middlewares;

namespace Warp.WebApp.Tests.Middlewares;

public class TraceContextMiddlewareTests
{
    [Fact]
    public async Task UsesExistingActivityAndPropagatesHeaders()
    {
        var original = Activity.Current;
        using var activity = new Activity("TestExistingActivity");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();

        try
        {
            var context = new DefaultHttpContext();
            var middleware = new TraceContextMiddleware(Next, NullLogger<TraceContextMiddleware>.Instance);

            await middleware.InvokeAsync(context);
            var traceId = activity.TraceId.ToString();

            Assert.Equal(traceId, context.TraceIdentifier);
            Assert.Equal(traceId, context.Response.Headers["x-trace-id"].ToString());
            Assert.Equal(activity.Id, context.Response.Headers.TraceParent.ToString());
        }
        finally
        {
            activity.Stop();
            Activity.Current = original;
        }

        
        static async Task Next(HttpContext ctx)
        {
            await ctx.Response.StartAsync();
            await ctx.Response.WriteAsync("ok");
        }
    }


    [Fact]
    public async Task CreatesActivityWhenMissing()
    {
        var original = Activity.Current;
        Activity.Current = null;

        try
        {
            var context = new DefaultHttpContext();
            var middleware = new TraceContextMiddleware(Next, NullLogger<TraceContextMiddleware>.Instance);

            await middleware.InvokeAsync(context);
            var traceId = context.TraceIdentifier;

            Assert.False(string.IsNullOrWhiteSpace(traceId));
            Assert.Equal(traceId, context.Response.Headers["x-trace-id"].ToString());
            Assert.False(string.IsNullOrWhiteSpace(context.Response.Headers.TraceParent.ToString()));
        }
        finally
        {
            Activity.Current = original;
        }

        
        static async Task Next(HttpContext ctx)
        {
            await ctx.Response.StartAsync();
            await ctx.Response.WriteAsync("ok");
        }
    }
}
