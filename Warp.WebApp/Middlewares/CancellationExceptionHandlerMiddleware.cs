using System.Diagnostics;
using Warp.WebApp.Telemetry.Logging;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Extensions;

namespace Warp.WebApp.Middlewares;

public class CancellationExceptionHandlerMiddleware
{
    public CancellationExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
        {
            var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
            var error = DomainErrors.ServiceUnavailable(traceId, ex.Message)
                .AddTraceId(traceId);

            var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<CancellationExceptionHandlerMiddleware>();
            logger.LogServiceUnavailable(traceId, ex.Message);

            await context.Response.WriteAsJsonAsync(error.ToProblemDetails());
            context.Response.StatusCode = error.Code.ToHttpStatusCodeInt();
        }
    }


    private readonly RequestDelegate _next;
}