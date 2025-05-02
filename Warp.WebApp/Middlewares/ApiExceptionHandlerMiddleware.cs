using Warp.WebApp.Telemetry.Logging;
using Warp.WebApp.Helpers;
using Warp.WebApp.Helpers.Configuration;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Extensions;

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
            var error = DomainErrors.ServerErrorWithMessage(ex.Message)
                .AddSentryId(sentryId)
                .AddTraceId(traceId);

            await HandleExceptionAsync(context, error, ex, _environment);
        }
    }


    private static Task HandleExceptionAsync(HttpContext context, DomainError error, Exception ex, IWebHostEnvironment environment)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = error.Code.ToHttpStatusCodeInt();

        var problemDetails = error.ToProblemDetails();
        if (environment.IsDevelopmentOrLocal())
            problemDetails.AddStackTrace(ex.StackTrace);

        return context.Response.WriteAsJsonAsync(problemDetails);
    }


    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ApiExceptionHandlerMiddleware> _logger;
    private readonly RequestDelegate _next;
}
