using System.Net;
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


        private const string ErrorsExtensionToken = "errors";
        private const string TraceIdExtensionToken = "trace-id";
    }
}
