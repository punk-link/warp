using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models.ProblemDetails;

namespace Warp.WebApp.Helpers
{
    public static class ProblemDetailsHelper
    {
        public static ProblemDetails Create(string detail, HttpStatusCode status = HttpStatusCode.BadRequest, string? type = null)
        {
            type ??= "about:blank";
            
            return new ProblemDetails
            {
                Detail = detail,
                Status = (int) status,
                Title = status.ToString(),
                Type = type,
            };
        }


        public static void AddErrors(this ProblemDetails details, List<Error> errors)
        {
            details.Extensions[ErrorsExtensionToken] = errors;
        }


        public static void AddTraceId(this ProblemDetails details, string traceId)
        {
            details.Extensions[TraceIdExtensionToken] = traceId;
        }


        public static List<Error> GetErrors(this ProblemDetails details)
        {
            var errorsObject = details.Extensions[ErrorsExtensionToken];
            if (errorsObject is null)
                return Enumerable.Empty<Error>().ToList();
            
            var jsonElement = (JsonElement) errorsObject;
            var errors = JsonSerializer.Deserialize<List<Error>>(jsonElement.GetRawText());
            
            return errors ?? Enumerable.Empty<Error>().ToList();
        }


        public static string GetTraceId(this ProblemDetails details)
        {
            return details.Extensions[TraceIdExtensionToken] as string ?? string.Empty;
        }


        private const string ErrorsExtensionToken = "errors";
        private const string TraceIdExtensionToken = "trace-id";
    }
}
