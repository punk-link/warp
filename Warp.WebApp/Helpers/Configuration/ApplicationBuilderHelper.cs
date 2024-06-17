using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;
using System.Net;

namespace Warp.WebApp.Helpers.Configuration;

public static class ApplicationBuilderHelper
{
    public static Action<IApplicationBuilder> ConfigureApiExceptionHandler(this IApplicationBuilder app, IWebHostEnvironment environment)
        => handler => handler.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error is null)
                return;

            var detail = exceptionHandlerPathFeature.Error.Message;
            const string type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1";
            var details = ProblemDetailsHelper.Create(detail, HttpStatusCode.InternalServerError, type);
            details.Instance = exceptionHandlerPathFeature.Endpoint?.ToString();

            details.AddTraceId(Activity.Current?.Id);

            if (environment.IsDevelopmentOrLocal())
                details.AddStackTrace(exceptionHandlerPathFeature.Error.StackTrace);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(details);
        });
}