﻿using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Services.Images;

public interface IImageService
{
    /// <summary>
    /// Retrieves metadata for a list of images associated with a specific entry.
    /// </summary>
    /// <param name="entryId">The ID of the entry containing the images.</param>
    /// <param name="imageIds">The list of image IDs to retrieve metadata for.</param>
    /// <returns>
    /// A result containing a list of image metadata if successful, 
    /// or problem details if the operation fails.
    /// </returns>
    public Task<Result<List<ImageInfo>, ProblemDetails>> GetAttached(Guid entryId, List<Guid> imageIds, CancellationToken cancellationToken);

    /// <summary>
    /// Removes an image from storage.
    /// </summary>
    /// <param name="entryId">The ID of the entry that owns the image.</param>
    /// <param name="imageId">The ID of the image to remove.</param>
    /// <returns>
    /// A unit result indicating success, or problem details if the operation fails.
    /// </returns>
    public Task<UnitResult<ProblemDetails>> Remove(Guid entryId, Guid imageId, CancellationToken cancellationToken);
}
