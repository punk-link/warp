namespace Warp.WebApp.Models;

public readonly record struct ImageInfo
{
    public Guid Id { get; init; }
    public byte[] Content { get; init; }
    public string ContentType { get; init; }
}