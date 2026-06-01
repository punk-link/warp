namespace Warp.WebApp.Models.Moderation;

/// <summary>
/// Represents the processing state of a content moderation check.
/// </summary>
public enum ModerationStatus
{
    /// <summary>Moderation has been scheduled but not yet executed.</summary>
    Pending = 0,

    /// <summary>Moderation completed successfully. See <see cref="ModerationResult.IsFlagged"/> for the outcome.</summary>
    Completed = 1,

    /// <summary>Moderation could not be completed due to a provider or processing error.</summary>
    Failed = 2
}
