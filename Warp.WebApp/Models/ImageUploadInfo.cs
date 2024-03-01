namespace Warp.WebApp.Models;

// TODO: use in the service
public readonly record struct ImageUploadInfo
{
    public Guid Id { get; init; }
    public string ClientFileName { get; init; }
}