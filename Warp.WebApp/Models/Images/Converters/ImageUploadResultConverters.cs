namespace Warp.WebApp.Models.Images.Converters;

public static class ImageUploadResultConverters
{
    public static ImageUploadResponse ToImageUploadResponse(this ImageUploadResult target, Uri partialUrl, Uri url)
        => new(target.Id, target.EntryId, target.ClientFileName, partialUrl, url);
}
