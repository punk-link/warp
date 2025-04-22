using System.Net;
using Warp.WebApp.Telemetry.Logging;
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
            var sentryId = SentrySdk.CaptureException(ex);

            var traceId = context.TraceIdentifier;
            _logger.LogServerErrorWithMessage(traceId, ex.Message);

            await HandleExceptionAsync(context, ex, traceId, sentryId, _environment);
        }
    }


    private static Task HandleExceptionAsync(HttpContext context, Exception ex, string? traceId, SentryId sentryId, IWebHostEnvironment environment)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var problemDetails = ProblemDetailsHelper.CreateServerException(ex.Message);
        problemDetails.AddTraceId(traceId)
            .AddSentryId(sentryId);

        if (environment.IsDevelopmentOrLocal())
            problemDetails.AddStackTrace(ex.StackTrace);

        return context.Response.WriteAsJsonAsync(problemDetails);
    }


    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ApiExceptionHandlerMiddleware> _logger;
    private readonly RequestDelegate _next;
}
