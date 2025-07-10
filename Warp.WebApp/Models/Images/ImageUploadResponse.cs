using Warp.WebApp.Services;

namespace Warp.WebApp.Models.Images;

public readonly record struct ImageUploadResponse
{
    public ImageUploadResponse(in Guid id, in Guid entryId, string clientFileName, Uri partialUrl, Uri url)
    { 
        Id = IdCoder.Encode(id);
        ClientFileName = clientFileName;
        EntryId = IdCoder.Encode(entryId);
        PartialUrl = partialUrl;
        Url = url;
    }


    public string Id { get; }
    public string ClientFileName { get; }
    public string EntryId { get; }
    public Uri PartialUrl { get; }
    public Uri Url { get; }
}
