namespace Warp.WebApp.Models;

public readonly record struct ImageResponse
{
    public required ImageInfo ImageInfo { get; init; }
    public required string ClientFileName { get; init; }
}

