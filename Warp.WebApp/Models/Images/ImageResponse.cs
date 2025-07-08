using Warp.WebApp.Services;

namespace Warp.WebApp.Models.Images;

public readonly record struct ImageResponse
{
    public ImageResponse(ImageInfo imageInfo, string clientFileName)
    {
        Id = imageInfo.Id;
        ClientFileName = clientFileName;
        EntryId = imageInfo.EntryId;
    }


    public Guid Id { get; }
    public string ClientFileName { get; }
    public Guid EntryId { get; }
}

