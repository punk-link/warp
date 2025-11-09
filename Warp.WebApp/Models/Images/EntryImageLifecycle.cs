using System.Text.Json.Serialization;

namespace Warp.WebApp.Models.Images;

/// <summary>
/// Represents lifecycle metadata for entry images scheduled for cleanup.
/// </summary>
public sealed record EntryImageLifecycle
{
    [JsonConstructor]
    public EntryImageLifecycle(Guid entryId, DateTime expiresAt, IReadOnlyList<Guid> imageIds, int failureCount)
    {
        EntryId = entryId;
        ExpiresAt = expiresAt;
        ImageIds = imageIds ?? [];
        FailureCount = failureCount;
    }


    public Guid EntryId { get; init; }


    public DateTime ExpiresAt { get; init; }


    public IReadOnlyList<Guid> ImageIds { get; init; }


    public int FailureCount { get; init; }


    public static EntryImageLifecycle Create(Guid entryId, DateTime expiresAt, IEnumerable<Guid> imageIds)
    {
        var images = imageIds?.ToArray() ?? [];
        return new EntryImageLifecycle(entryId, expiresAt, images, 0);
    }


    public EntryImageLifecycle IncrementFailure(DateTime nextAttempt)
        => this with { FailureCount = FailureCount + 1, ExpiresAt = nextAttempt };


    public EntryImageLifecycle WithImages(IEnumerable<Guid> imageIds, DateTime expiresAt)
    {
        var images = imageIds?.ToArray() ?? [];
        return this with { ImageIds = images, ExpiresAt = expiresAt, FailureCount = 0 };
    }
}
