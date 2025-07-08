using CSharpFunctionalExtensions;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Models.Images.Converters;

public static class ImageInfoConverters
{
    public static async Task<Result<ImageResponse, DomainError>> ToImageResponse(this Task<Result<ImageInfo, DomainError>> target, string? clientFileName)
    {
        var result = await target;
        return result.ToImageResponse(clientFileName);
    }


    public static Result<ImageResponse, DomainError> ToImageResponse(this Result<ImageInfo, DomainError> target, string? clientFileName)
    {
        if (target.IsFailure)
            return Result.Failure<ImageResponse, DomainError>(target.Error);

        return Result.Success<ImageResponse, DomainError>(target.Value.ToImageResponse(clientFileName));
    }


    public static ImageResponse ToImageResponse(this ImageInfo target, string? clientFileName) 
        => new(target, clientFileName ?? "unknown");
}
