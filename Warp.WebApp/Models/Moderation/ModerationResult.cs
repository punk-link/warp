using System.Text.Json.Serialization;

namespace Warp.WebApp.Models.Moderation;

/// <summary>
/// Stores the outcome of a single content moderation check for a text or image item.
/// </summary>
public readonly record struct ModerationResult
{
    /// <summary>
    /// Creates a <see cref="ModerationResult"/> in the <see cref="ModerationStatus.Pending"/> state.
    /// </summary>
    public static ModerationResult CreatePending()
        => new() { Status = ModerationStatus.Pending };


    /// <summary>
    /// Creates a <see cref="ModerationResult"/> in the <see cref="ModerationStatus.Completed"/> state.
    /// </summary>
    /// <param name="isFlagged">Whether the content was flagged by the provider.</param>
    /// <param name="categoryScores">Provider-supplied per-category confidence scores, if available.</param>
    public static ModerationResult CreateCompleted(bool isFlagged, IReadOnlyDictionary<string, double>? categoryScores)
        => new()
        {
            Status = ModerationStatus.Completed,
            IsFlagged = isFlagged,
            CategoryScores = categoryScores,
            CompletedAt = DateTimeOffset.UtcNow
        };


    /// <summary>
    /// Creates a <see cref="ModerationResult"/> in the <see cref="ModerationStatus.Failed"/> state.
    /// </summary>
    public static ModerationResult CreateFailed()
        => new() { Status = ModerationStatus.Failed };


    /// <summary>Gets the current moderation processing state.</summary>
    public ModerationStatus Status { get; init; }

    /// <summary>
    /// Indicates whether the provider flagged this content.
    /// Only meaningful when <see cref="Status"/> is <see cref="ModerationStatus.Completed"/>.
    /// </summary>
    public bool IsFlagged { get; init; }

    /// <summary>
    /// Provider-supplied confidence scores per moderation category.
    /// May be <c>null</c> when the provider does not return scores or moderation has not completed.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, double>? CategoryScores { get; init; }

    /// <summary>
    /// The UTC timestamp when moderation was completed.
    /// <c>null</c> when <see cref="Status"/> is not <see cref="ModerationStatus.Completed"/>.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? CompletedAt { get; init; }
}
