namespace Warp.WebApp.Models.Images;

public readonly record struct ImageUploadResult
{
    public ImageUploadResult(in ImageInfo imageInfo, string clientFileName)
    {
        Id = imageInfo.Id;
        ClientFileName = clientFileName;
        EntryId = imageInfo.EntryId;
    }


    public Guid Id { get; }
    public string ClientFileName { get; }
    public Guid EntryId { get; }
}

