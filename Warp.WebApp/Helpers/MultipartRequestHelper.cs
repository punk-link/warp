using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Net.Http.Headers;

namespace Warp.WebApp.Helpers;

public class MultipartRequestHelper
{
    public static Result<string, ProblemDetails> GetBoundary(IStringLocalizer<ServerResources> localizer, MediaTypeHeaderValue contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
            return ProblemDetailsHelper.Create(localizer["Missing content-type boundary."]);

        if (boundary.Length > lengthLimit)
            return ProblemDetailsHelper.Create(localizer["Multipart boundary length limit exceeded."]);

        return boundary;
    }


    public static bool IsMultipartContentType(string contentType) 
        => !string.IsNullOrEmpty(contentType) && contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);


    public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition) 
        // Content-Disposition: form-data; name="key";
        => contentDisposition is not null
            && contentDisposition.DispositionType.Equals("form-data")
            && string.IsNullOrEmpty(contentDisposition.FileName.Value)
            && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);


    public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition) 
        // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
        => contentDisposition is not null
            && contentDisposition.DispositionType.Equals("form-data")
            && (!string.IsNullOrEmpty(contentDisposition.FileName.Value) || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
}
