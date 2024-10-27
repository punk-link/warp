using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Text.Json;
using Warp.WebApp.Models.ProblemDetails;

namespace Warp.WebApp.Helpers;

public static class ProblemDetailsHelper
{
    public static void AddErrors(this ProblemDetails details, List<Error> errors)
    {
        details.Extensions[ErrorsExtensionToken] = errors;
    }


    public static void AddStackTrace(this ProblemDetails details, string? stackTrace)
    {
        if (string.IsNullOrWhiteSpace(stackTrace))
            return;

        details.Extensions[StackTraceExtensionToken] = stackTrace;
    }


    public static void AddTraceId(this ProblemDetails details, string? traceId)
    {
        if (string.IsNullOrWhiteSpace(traceId))
            return;

        details.Extensions[TraceIdExtensionToken] = traceId;
    }


    public static ProblemDetails Create(string detail, HttpStatusCode status = HttpStatusCode.BadRequest, string? type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6")
        => new()
        {
            Detail = detail,
            Status = (int)status,
            Title = status.ToString(),
            Type = type,
        };


    public static ProblemDetails CreateNotFound(IStringLocalizer<ServerResources> localizer)
        => Create(localizer["NotFoundErrorMessage"], HttpStatusCode.NotFound, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4");


    public static ProblemDetails CreateServerException(string error)
        => Create(error, HttpStatusCode.InternalServerError, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1");


    public static ProblemDetails CreateServiceUnavailable(IStringLocalizer<ServerResources> localizer)
        => Create(localizer["ServiceUnavailableErrorMessage"], HttpStatusCode.ServiceUnavailable, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4");


    public static ProblemDetails CreateForbidden(IStringLocalizer<ServerResources> localizer)
        => Create(localizer["NoPermissionErrorMessage"], HttpStatusCode.Forbidden, "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3");


    public static ProblemDetails CreateUnauthorized(IStringLocalizer<ServerResources> localizer, string? detail = null)
        => Create(detail ?? localizer["UnauthorizedErrorMessage"], HttpStatusCode.Unauthorized, "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1");


    public static List<Error> GetErrors(this ProblemDetails details)
    {
        if (!details.Extensions.TryGetValue(ErrorsExtensionToken, out var errorsObject))
            return Enumerable.Empty<Error>().ToList();

        if (errorsObject is null)
            return Enumerable.Empty<Error>().ToList();

        if (errorsObject is JsonElement jsonElement)
            return JsonSerializer.Deserialize<List<Error>>(jsonElement.GetRawText())!;

        return (List<Error>)errorsObject;
    }


    public static string GetTraceId(this ProblemDetails details)
    {
        if (!details.Extensions.TryGetValue(TraceIdExtensionToken, out var traceId))
            return string.Empty;

        var traceIdString = traceId?.ToString();

        return traceIdString ?? string.Empty;
    }


    private const string ErrorsExtensionToken = "errors";
    private const string StackTraceExtensionToken = "stack-trace";
    private const string TraceIdExtensionToken = "trace-id";
}