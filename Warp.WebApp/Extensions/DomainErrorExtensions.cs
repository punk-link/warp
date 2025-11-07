using Microsoft.AspNetCore.Mvc;
using Polly;
using System.Diagnostics;
using Warp.WebApp.Constants;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Extensions;

public static class DomainErrorExtensions
{
    public static DomainError AddSentryId(this DomainError error, SentryId sentryId)
    {
        if (sentryId == SentryId.Empty)
            return error;

        error.Extensions[ErrorExtensionKeys.SentryIdExtensionToken] = sentryId.ToString();
        return error;
    }


    public static DomainError AddTraceId(this DomainError error, string? traceId)
    {
        if (string.IsNullOrWhiteSpace(traceId))
            return error;
     
        error.Extensions[ErrorExtensionKeys.TraceIdExtensionToken] = traceId;
        return error;
    }


    public static ProblemDetails ToProblemDetails(this DomainError error)
    {
        var httpStatusCode = error.Code.ToHttpStatusCode();
        
        var traceId = Activity.Current?.TraceId.ToString();
        error.AddTraceId(traceId);

        var problemDetails = ProblemDetailsHelper.Create(httpStatusCode, error.Detail);
        problemDetails.Extensions[ErrorExtensionKeys.EventId] = (int)error.Code;

        foreach (var extension in error.Extensions)
           problemDetails.Extensions[extension.Key] = extension.Value;

        return problemDetails;
    }
}
