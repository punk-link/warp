namespace Warp.WebApp.Services.Entries;

/// <summary>
/// Defines methods for managing abuse reports in the application.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Checks if an entity is reported.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the report exists.</returns>
    public ValueTask<bool> Contains(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Marks an entity with the specified identifier as reported.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task MarkAsReported(Guid id, CancellationToken cancellationToken);
}