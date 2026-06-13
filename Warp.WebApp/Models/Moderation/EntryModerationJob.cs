using System.Text.Json.Serialization;

namespace Warp.WebApp.Models.Moderation;

/// <summary>
/// Tracks the pending and in-flight content moderation work for a single entry.
/// </summary>
public sealed record EntryModerationJob
{
    [JsonConstructor]
    public EntryModerationJob(
        Guid entryId,
        bool needsTextModeration,
        List<Guid> pendingImageIds,
        int failureCount,
        DateTimeOffset expiresAt,
        DateTimeOffset nextAttemptAt = default)
    {
        EntryId = entryId;
        FailureCount = failureCount;
        ExpiresAt = expiresAt;
        FailureCount = failureCount;
        NeedsTextModeration = needsTextModeration;
        NextAttemptAt = nextAttemptAt == default ? expiresAt : nextAttemptAt;
        PendingImageIds = pendingImageIds;
    }


    /// <summary>Creates a new job with all items pending.</summary>
    public static EntryModerationJob Create(Guid entryId, bool needsTextModeration, List<Guid> imageIds, DateTimeOffset expiresAt, DateTimeOffset nextAttemptAt)
        => new(entryId, needsTextModeration, imageIds, failureCount: 0, expiresAt, nextAttemptAt);

    
    /// <summary>Returns a copy of this job with the failure counter incremented and a new scheduled time.</summary>
    public EntryModerationJob IncrementFailure(DateTimeOffset nextAttemptAt)
        => this with { FailureCount = FailureCount + 1, NextAttemptAt = nextAttemptAt };


    /// <summary>Returns a copy of this job with only the items that still need processing.</summary>
    public EntryModerationJob WithRemainingWork(bool needsTextModeration, List<Guid> pendingImageIds)
        => this with { NeedsTextModeration = needsTextModeration, PendingImageIds = pendingImageIds };


    /// <summary>The identifier of the entry this job is tracking.</summary>
    public Guid EntryId { get; init; }

    /// <summary>The entry expiration time, used as the TTL anchor for the persisted moderation job.</summary>
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>How many times this job has failed consecutively.</summary>
    public int FailureCount { get; init; }

    /// <summary>Whether the text content of the entry still needs to be moderated.</summary>
    public bool NeedsTextModeration { get; init; }

    /// <summary>The next UTC time at which the moderation job should be attempted.</summary>
    public DateTimeOffset NextAttemptAt { get; init; }

    /// <summary>Image IDs that still need to be moderated.</summary>
    public List<Guid> PendingImageIds { get; init; }
}
