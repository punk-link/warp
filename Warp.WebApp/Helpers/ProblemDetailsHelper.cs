using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using Warp.WebApp.Constants;
using Warp.WebApp.Models.ProblemDetails;

namespace Warp.WebApp.Helpers;

public static class ProblemDetailsHelper
{
    public static ProblemDetails AddStackTrace(this ProblemDetails details, string? stackTrace)
    {
        if (string.IsNullOrWhiteSpace(stackTrace))
            return details;

        details.Extensions[ErrorExtensionKeys.StackTraceExtensionToken] = stackTrace;
        return details;
    }


    public static ProblemDetails Create(HttpStatusCode status, string detail)
        => Create(detail, status, GetType(status));


    public static ProblemDetails Create(string detail, HttpStatusCode status = HttpStatusCode.BadRequest, string? type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6")
        => new()
        {
            Detail = detail,
            Status = (int)status,
            Title = status.ToString(),
            Type = type,
        };


    public static List<Error> GetErrors(this ProblemDetails details)
    {
        if (!details.Extensions.TryGetValue(ErrorExtensionKeys.ErrorsExtensionToken, out var errorsObject))
            return Enumerable.Empty<Error>().ToList();

        if (errorsObject is null)
            return Enumerable.Empty<Error>().ToList();

        if (errorsObject is JsonElement jsonElement)
            return JsonSerializer.Deserialize<List<Error>>(jsonElement.GetRawText())!;

        return (List<Error>)errorsObject;
    }


    public static string GetTraceId(this ProblemDetails details)
    {
        if (!details.Extensions.TryGetValue(ErrorExtensionKeys.TraceIdExtensionToken, out var traceId))
            return string.Empty;

        var traceIdString = traceId?.ToString();
        return traceIdString ?? string.Empty;
    }


    private static string GetType(HttpStatusCode statusCode) 
        => statusCode switch
        {
            HttpStatusCode.BadRequest => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            HttpStatusCode.NotFound => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4",
            HttpStatusCode.InternalServerError => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            HttpStatusCode.ServiceUnavailable => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.4",
            HttpStatusCode.Forbidden => "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
            HttpStatusCode.Unauthorized => "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
            _ => "https://datatracker.ietf.org/doc/html/rfc7231#section-6",
        };
}