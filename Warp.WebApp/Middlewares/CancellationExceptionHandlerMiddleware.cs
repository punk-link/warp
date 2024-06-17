using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Diagnostics;
using Warp.WebApp.Extensions.Logging;
using Warp.WebApp.Helpers;

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
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            LogException(context, ex, traceId);
            var problemDetails = BuildProblemDetails(context, traceId);

            await context.Response.WriteAsJsonAsync(problemDetails);
            context.Response.StatusCode = problemDetails.Status!.Value;
        }
    }


    private static ProblemDetails BuildProblemDetails(HttpContext context, string traceId)
    {
        var localizer = context.RequestServices.GetRequiredService<IStringLocalizer<ServerResources>>();

        var result = ProblemDetailsHelper.CreateServiceUnavailable(localizer);
        result.AddTraceId(traceId);

        return result;
    }


    private static void LogException(HttpContext context, Exception exception, string traceId)
    {
        var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<CancellationExceptionHandlerMiddleware>();
        logger.LogServiceUnavailable(traceId, exception.Message);
    }


    private readonly RequestDelegate _next;
}