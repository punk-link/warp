using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Helpers.Configuration;

public static class ApplicationBuilderHelper
{
    public static Action<IApplicationBuilder> ConfigureApiExceptionHandler(this IApplicationBuilder _, IWebHostEnvironment environment)
        => handler => handler.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error is null)
                return;

            var error = DomainErrors.ServerErrorWithMessage(exceptionHandlerPathFeature.Error.Message)
                .AddTraceId(Activity.Current?.TraceId.ToString());

            var details = error.ToProblemDetails();
            details.Instance = exceptionHandlerPathFeature.Endpoint?.ToString();
            if (environment.IsDevelopmentOrLocal())
                details.AddStackTrace(exceptionHandlerPathFeature.Error.StackTrace);

            context.Response.StatusCode = error.Code.ToHttpStatusCodeInt();
            await context.Response.WriteAsJsonAsync(details);
        });
}