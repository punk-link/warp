namespace Warp.WebApp.Models;

public readonly record struct ImageUploadInfo
{
    public Guid Id { get; init; }
    public string ClientFileName { get; init; }
}