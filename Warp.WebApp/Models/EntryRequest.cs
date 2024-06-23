namespace Warp.WebApp.Models;

public readonly record struct EntryRequest
{
    public string TextContent { get; init; }
    public TimeSpan ExpiresIn { get; init; }
    public List<Guid> ImageIds { get; init; }
}
