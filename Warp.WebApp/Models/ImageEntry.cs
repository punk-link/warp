namespace Warp.WebApp.Models;

public readonly record struct ImageEntry
{
    public Guid Id { get; init; }
    public byte[] Content { get; init; }
}