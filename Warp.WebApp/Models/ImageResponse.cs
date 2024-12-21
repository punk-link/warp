namespace Warp.WebApp.Models;

public readonly record struct ImageResponse
{
    public ImageResponse(ImageInfo imageInfo, string clientFileName)
    {
        ClientFileName = clientFileName;
        ImageInfo = imageInfo;
    }


    public ImageInfo ImageInfo { get; }
    public string ClientFileName { get; }
}

