using CSharpFunctionalExtensions;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Models.Images.Converters;

public static class ImageInfoConverters
{
    public static List<ImageInfoResponse> ToImageInfoResponse(this ICollection<ImageInfo> imageInfos)
        => [.. imageInfos.Select(imageInfo => imageInfo.ToImageInfoResponse())];


    public static ImageInfoResponse ToImageInfoResponse(this ImageInfo imageInfo)
        => new(imageInfo);


    public static async Task<Result<ImageUploadResult, DomainError>> ToImageResponse(this Task<Result<ImageInfo, DomainError>> target, string? clientFileName)
    {
        var result = await target;
        return result.ToImageResponse(clientFileName);
    }


    public static Result<ImageUploadResult, DomainError> ToImageResponse(this Result<ImageInfo, DomainError> target, string? clientFileName)
    {
        if (target.IsFailure)
            return Result.Failure<ImageUploadResult, DomainError>(target.Error);

        return Result.Success<ImageUploadResult, DomainError>(target.Value.ToImageResponse(clientFileName));
    }


    public static ImageUploadResult ToImageResponse(this ImageInfo target, string? clientFileName) 
        => new(target, clientFileName ?? "unknown");
}
