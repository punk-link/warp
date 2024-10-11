using OpenTelemetry.Trace;
using System.Reflection;
using Warp.WebApp.Attributes;

namespace Warp.WebApp.Middlewares;

public class TraceMethodMiddleware
{
    public TraceMethodMiddleware(RequestDelegate next, TracerProvider tracerProvider)
    {
        _next = next;
        _tracer = tracerProvider.GetTracer("TraceMiddleware");
    }


    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is not null)
        {
            var methodInfo = endpoint.Metadata.GetMetadata<MethodInfo>();
            var traceMethodAttribute = methodInfo?.GetCustomAttribute<TraceMethodAttribute>();
            if (traceMethodAttribute is not null)
            {
                using var span = _tracer.StartActiveSpan(methodInfo!.Name);
                if (traceMethodAttribute.Tags.Count != 0)
                {
                    foreach (var tag in traceMethodAttribute.Tags)
                        span.SetAttribute(tag.Key, tag.Value);
                }

                await _next(context);
                span.End();

                return;
            }
        }

        await _next(context);
    }


    private readonly RequestDelegate _next;
    private readonly Tracer _tracer;
}
