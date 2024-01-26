namespace Warp.WebApp.Models;

public readonly record struct ImageBucket
{
    public Guid Id { get; init; }
    public List<ImageEntry> Images { get; init; }
}