using System.Net;
using Warp.WebApp.Extensions.Logging;
using Warp.WebApp.Helpers;
using Warp.WebApp.Helpers.Configuration;

namespace Warp.WebApp.Middlewares;

public class ApiExceptionHandlerMiddleware
{
    public ApiExceptionHandlerMiddleware(RequestDelegate next, ILogger<ApiExceptionHandlerMiddleware> logger, IWebHostEnvironment environment)
    {
        _environment = environment;
        _logger = logger;
        _next = next;
    }


    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;
            _logger.LogGenericServerError(traceId, ex.Message);

            SentrySdk.CaptureException(ex);

            await HandleExceptionAsync(context, ex, traceId, _environment);
        }
    }


    private static Task HandleExceptionAsync(HttpContext context, Exception ex, string? traceId, IWebHostEnvironment environment)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var problemDetails = ProblemDetailsHelper.CreateServerException(ex.Message);
        problemDetails.AddTraceId(traceId);

        if (environment.IsDevelopmentOrLocal())
            problemDetails.AddStackTrace(ex.StackTrace);

        return context.Response.WriteAsJsonAsync(problemDetails);
    }


    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ApiExceptionHandlerMiddleware> _logger;
    private readonly RequestDelegate _next;
}
