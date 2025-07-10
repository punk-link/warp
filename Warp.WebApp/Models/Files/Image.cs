namespace Warp.WebApp.Models.Files;

public readonly record struct Image
{
    public Guid Id { get; init; }
    public Stream Content { get; init; }
    public string ContentType { get; init; }
}
