﻿using CSharpFunctionalExtensions;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Files;

namespace Warp.WebApp.Services.Images;

public interface IUnauthorizedImageService
{
    /// <summary>
    /// Adds a new image associated with a specific entry.
    /// </summary>
    /// <param name="entryId">The ID of the entry to associate the image with.</param>
    /// <param name="appFile">The file to be uploaded as an image.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A result containing the image response with image metadata if successful, 
    /// or problem details if the operation fails.
    /// </returns>
    Task<Result<ImageResponse, DomainError>> Add(Guid entryId, AppFile appFile, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves an image by its ID and associated entry ID.
    /// </summary>
    /// <param name="entryId">The ID of the entry that owns the image.</param>
    /// <param name="imageId">The ID of the image to retrieve.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>
    /// A result containing the retrieved image with content if successful, 
    /// or problem details if the operation fails.
    /// </returns>
    public Task<Result<Image, DomainError>> Get(Guid entryId, Guid imageId, CancellationToken cancellationToken);
}