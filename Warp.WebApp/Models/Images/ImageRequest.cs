namespace Warp.WebApp.Models.Images;

public readonly record struct ImageRequest
{
    public required Guid Id { get; init; }
    public required string ContentType { get; init; }
    public required Stream Content { get; init; }
    public required Guid EntryId { get; init; }
}
