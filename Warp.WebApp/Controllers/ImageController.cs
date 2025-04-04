using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using System.Text.Json;
using Warp.WebApp.Attributes;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.Infrastructure;

namespace Warp.WebApp.Controllers;

/// <summary>
/// Handles image-related operations including retrieval, uploading, and deletion of images.
/// </summary>
[ApiController]
[Route("/api/images")]
public sealed class ImageController : BaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageController"/> class.
    /// </summary>
    /// <param name="options">Configuration options for image uploads</param>
    /// <param name="cookieService">Service for handling cookies</param>
    /// <param name="creatorService">Service for managing creators</param>
    /// <param name="entryInfoService">Service for managing entry information</param>
    /// <param name="loggerFactory">Factory for creating loggers</param>
    /// <param name="partialViewRenderHelper">Service for rendering partial views</param>
    /// <param name="unauthorizedImageService">Service for handling images without authorization</param>
    /// <param name="localizer">Service for handling text localization</param>
    public ImageController(IOptions<ImageUploadOptions> options,
        ICookieService cookieService, 
        ICreatorService creatorService,
        IEntryInfoService entryInfoService,
        ILoggerFactory loggerFactory,
        IPartialViewRenderService partialViewRenderHelper,
        IUnauthorizedImageService unauthorizedImageService,
        IStringLocalizer<ServerResources> localizer) 
        : base(localizer, cookieService, creatorService)
    {
        _entryInfoService = entryInfoService;
        _localizer = localizer;
        _loggerFactory = loggerFactory;
        _options = options.Value;
        _partialViewRenderHelper = partialViewRenderHelper;
        _unauthorizedImageService = unauthorizedImageService;
    }


    /// <summary>
    /// Retrieves an image by its entry and image identifiers.
    /// </summary>
    /// <param name="entryId">Encoded identifier of the entry containing the image</param>
    /// <param name="imageId">Encoded identifier of the image to retrieve</param>
    /// <returns>
    /// The requested image as a file stream or a bad request response if the image cannot be retrieved.
    /// </returns>
    /// <remarks>
    /// This endpoint is cached for 10 minutes and varies by the route values entryId and imageId.
    /// </remarks>
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpGet("entry-id/{entryId}/image-id/{imageId}")]
    [OutputCache(Duration = 10 * 60, VaryByRouteValueNames = ["entryId", "imageId"])]
    public async Task<IActionResult> Get([FromRoute] string entryId, [FromRoute] string imageId, CancellationToken cancellationToken = default)
    {
        var decodedEntryId = IdCoder.Decode(entryId);
        if (decodedEntryId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var decodedImageId = IdCoder.Decode(imageId);
        if (decodedImageId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var (_, isFailure, value, error) = await _unauthorizedImageService.Get(decodedEntryId, decodedImageId, cancellationToken);
        if (isFailure)
            return BadRequest(error);

        return new FileStreamResult(value.Content, value.ContentType);
    }


    /// <summary>
    /// Removes an image from an entry.
    /// </summary>
    /// <param name="entryId">Encoded identifier of the entry containing the image</param>
    /// <param name="imageId">Encoded identifier of the image to remove</param>
    /// <returns>
    /// A no content (204) response if successful, forbidden (403) if the user is not authorized,
    /// or a bad request if the identifiers cannot be decoded.
    /// </returns>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpDelete("entry-id/{entryId}/image-id/{imageId}")]
    public async Task<IActionResult> Remove([FromRoute] string entryId, [FromRoute] string imageId, CancellationToken cancellationToken = default)
    {
        var decodedEntryId = IdCoder.Decode(entryId);
        if (decodedEntryId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var decodedImageId = IdCoder.Decode(imageId);
        if (decodedImageId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var creatorResult = await GetCreator(cancellationToken);
        if (creatorResult.IsFailure)
            return ReturnForbid();

        await _entryInfoService.RemoveImage(creatorResult.Value, decodedEntryId, decodedImageId, cancellationToken);
        return NoContent();
    }


    /// <summary>
    /// Uploads one or more images to an entry.
    /// </summary>
    /// <param name="entryId">Encoded identifier of the entry to which the images will be uploaded</param>
    /// <returns>
    /// Dictionary mapping file names to either rendered HTML content (for successful uploads)
    /// or error information (for failed uploads).
    /// </returns>
    /// <remarks>
    /// This endpoint accepts multipart form data with a file size limit of 50MB.
    /// It processes each file in the multipart request, validates it, and associates it with the entry.
    /// </remarks>
    [MultipartFormData]
    [DisableFormValueModelBinding]
    [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)] // 50MB
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(ImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost("entry-id/{entryId}")]
    public async Task<IActionResult> Upload([FromRoute] string entryId, CancellationToken cancellationToken = default)
    {
        Debug.Assert(Request.ContentType is not null, "Content type is not null because of the [MultipartFormData] attribute");

        var decodedEntryId = IdCoder.Decode(entryId);
        if (decodedEntryId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var boundaryResult = MultipartRequestHelper.GetBoundary(
            _localizer, 
            MediaTypeHeaderValue.Parse(Request.ContentType), 
            _options.RequestBoundaryLengthLimit);
        
        if (boundaryResult.IsFailure)
            return BadRequest(boundaryResult.Error);

        var reader = new MultipartReader(boundaryResult.Value, HttpContext.Request.Body);
        if (reader is null)
            return BadRequest(ProblemDetailsHelper.Create(_localizer["Failed to create MultipartReader"]));

        var uploadResults = await ProcessUploadedFiles(reader, decodedEntryId, cancellationToken);
    
        return Ok(await BuildUploadResults(uploadResults));
    }


    private async Task<Dictionary<string, string>> BuildUploadResults(List<Result<ImageResponse, ProblemDetails>> uploadResults)
    {
        var results = new Dictionary<string, string>();
        foreach (var (_, isFailure, imageResponse, error) in uploadResults)
        {
            if (isFailure)
            {
                var fileName = error.Extensions[ProblemDetailsFileNameExtensionKey] as string ?? "unknown";

                var errorJson = JsonSerializer.Serialize(error);
                results.Add(fileName, errorJson);
                continue;
            }

            var partialView = new PartialViewResult
            {
                
                ViewName = "Components/EditableImageContainer",
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = new EditableImageContainerModel(imageResponse.ImageInfo)
                }
            };

            var renderResult = await _partialViewRenderHelper.Render(ControllerContext, HttpContext, partialView);
            results.Add(imageResponse.ClientFileName, renderResult);
        }

        return results;
    }


    private async Task<List<Result<ImageResponse, ProblemDetails>>> ProcessUploadedFiles(MultipartReader reader, Guid decodedEntryId, CancellationToken cancellationToken)
    {
        var fileHelper = new FileHelper(_loggerFactory, _localizer, _options.AllowedExtensions, _options.MaxFileSize);

        var uploadResults = new List<Result<ImageResponse, ProblemDetails>>();
        var uploadedFilesCount = 0;
        MultipartSection? section;

        do
        {
            if (_options.MaxFileCount <= uploadedFilesCount)
                break;

            section = await reader.ReadNextSectionAsync(cancellationToken);
            if (section is null)
                break;

            var (HasValue, Value) = await ProcessSection(section, fileHelper, decodedEntryId, cancellationToken);
            if (!HasValue)
                continue;
            
            uploadResults.Add(Value);
            uploadedFilesCount++;
        } while (section is not null);

        return uploadResults;
    }


    private async Task<(bool HasValue, Result<ImageResponse, ProblemDetails> Value)> ProcessSection(MultipartSection section, FileHelper fileHelper, Guid decodedEntryId, CancellationToken cancellationToken)
    {
        if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
            return (false, default);

        if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
            return (false, default);

        var appFileResult = await fileHelper.ProcessStreamedFile(section, contentDisposition);
        if (appFileResult.IsFailure)
        {
            var enrichedError = EnrichProblemDetails(contentDisposition, appFileResult.Error);
            return (true, Result.Failure<ImageResponse, ProblemDetails>(enrichedError));
        }

        var uploadResult = await _unauthorizedImageService.Add(decodedEntryId, appFileResult.Value, cancellationToken);
        return (true, uploadResult);


        static ProblemDetails EnrichProblemDetails(ContentDispositionHeaderValue contentDisposition, ProblemDetails problemDetails)
        {
            problemDetails.Extensions.Add(ProblemDetailsFileNameExtensionKey, contentDisposition.FileName.Value);
            return problemDetails;
        }
    }


    private const string ProblemDetailsFileNameExtensionKey = "fileName";

    private readonly IEntryInfoService _entryInfoService;
    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ImageUploadOptions _options;
    private readonly IPartialViewRenderService _partialViewRenderHelper;
    private readonly IUnauthorizedImageService _unauthorizedImageService;
}
