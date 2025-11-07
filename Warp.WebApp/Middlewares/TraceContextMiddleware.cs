using System.Diagnostics;

namespace Warp.WebApp.Middlewares;

/// <summary>
/// Ensures incoming trace context is available throughout the request pipeline and emitted with responses.
/// </summary>
public sealed class TraceContextMiddleware
{
    public TraceContextMiddleware(RequestDelegate next, ILogger<TraceContextMiddleware> logger)
    {
        _logger = logger;
        _next = next;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        var activity = Activity.Current;
        Activity? startedActivity = null;

        if (activity is null)
            InitializeActivityFromRequestHeaders(context, out activity, out startedActivity);

        if ((activity.ActivityTraceFlags & ActivityTraceFlags.Recorded) == 0)
            activity.ActivityTraceFlags |= ActivityTraceFlags.Recorded;

        var traceId = activity.TraceId.ToString();
        context.TraceIdentifier = traceId;
        context.Items[TraceIdItemKey] = traceId;

        ApplyResponseHeaders(context.Response.Headers, activity, traceId);

        context.Response.OnStarting(() =>
        {
            ApplyResponseHeaders(context.Response.Headers, activity, traceId);
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["TraceId"] = traceId,
            ["SpanId"] = activity.SpanId.ToString()
        }))
        {
            await _next(context);
        }

        startedActivity?.Stop();
    }


    private static void ApplyResponseHeaders(IHeaderDictionary headers, Activity activity, string traceId)
    {
        headers[TraceIdHeaderName] = traceId;

        if (!string.IsNullOrEmpty(activity.Id))
            headers[TraceParentHeaderName] = activity.Id!;

        if (!string.IsNullOrEmpty(activity.TraceStateString))
            headers[TraceStateHeaderName] = activity.TraceStateString!;
    }


    private static void InitializeActivityFromRequestHeaders(HttpContext context, out Activity activity, out Activity? startedActivity)
    {
        startedActivity = new Activity(ActivityName)
            .SetIdFormat(ActivityIdFormat.W3C);

        var traceParentHeader = context.Request.Headers[TraceParentHeaderName].ToString();
        if (!string.IsNullOrWhiteSpace(traceParentHeader))
            startedActivity.SetParentId(traceParentHeader);

        var traceStateHeader = context.Request.Headers[TraceStateHeaderName].ToString();
        if (!string.IsNullOrWhiteSpace(traceStateHeader))
            startedActivity.TraceStateString = traceStateHeader;

        startedActivity.ActivityTraceFlags |= ActivityTraceFlags.Recorded;
        activity = startedActivity.Start();
    }


    private const string ActivityName = "Warp.WebApp.Request";
    private const string TraceParentHeaderName = "traceparent";
    private const string TraceStateHeaderName = "tracestate";
    private const string TraceIdHeaderName = "x-trace-id";
    private const string TraceIdItemKey = "TraceId";

    private readonly ILogger<TraceContextMiddleware> _logger;
    private readonly RequestDelegate _next;
}
