using CSharpFunctionalExtensions;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Files;

namespace Warp.WebApp.Data.S3;

/// <summary>
/// Defines operations for managing files in Amazon S3 storage.
/// </summary>
public interface IS3FileStorage
{
    /// <summary>
    /// Checks if files with the specified keys exist in the given prefix.
    /// </summary>
    /// <param name="prefix">The S3 prefix (folder path) to check.</param>
    /// <param name="keys">List of file keys to check for existence.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing a set of keys that exist in S3, or a domain error if the operation fails.</returns>
    Task<Result<HashSet<string>, DomainError>> Contains(string prefix, List<string> keys, CancellationToken cancellationToken);
    
    /// <summary>
    /// Deletes a file from S3 storage.
    /// </summary>
    /// <param name="prefix">The S3 prefix (folder path) where the file is located.</param>
    /// <param name="key">The key (filename) of the file to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result indicating success or a domain error if the operation fails.</returns>
    public Task<UnitResult<DomainError>> Delete(string prefix, string key, CancellationToken cancellationToken);
    
    /// <summary>
    /// Retrieves a file from S3 storage.
    /// </summary>
    /// <param name="prefix">The S3 prefix (folder path) where the file is located.</param>
    /// <param name="key">The key (filename) of the file to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the retrieved file as an AppFile, or a domain error if the operation fails.</returns>
    public Task<Result<AppFile, DomainError>> Get(string prefix, string key, CancellationToken cancellationToken);
    
    /// <summary>
    /// Saves a file to S3 storage.
    /// </summary>
    /// <param name="prefix">The S3 prefix (folder path) where the file will be saved.</param>
    /// <param name="key">The key (filename) to use for the saved file.</param>
    /// <param name="appFile">The file content and metadata to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result indicating success or a domain error if the operation fails.</returns>
    public Task<UnitResult<DomainError>> Save(string prefix, string key, AppFile appFile, CancellationToken cancellationToken);
}
