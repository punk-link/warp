using CSharpFunctionalExtensions;
using Microsoft.Net.Http.Headers;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Errors;

namespace Warp.WebApp.Helpers;

public class MultipartRequestHelper
{
    public static Result<string, DomainError> GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
            return DomainErrors.MultipartContentTypeBoundaryError();

        if (boundary.Length > lengthLimit)
            return DomainErrors.MultipartBoundaryLengthLimitExceeded();

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
