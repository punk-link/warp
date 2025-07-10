using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Diagnostics;
using Warp.WebApp.Attributes;
using Warp.WebApp.Constants;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Models.Images.Converters;
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
    public ImageController(IOptions<ImageUploadOptions> options,
        ICookieService cookieService, 
        ICreatorService creatorService,
        IEntryInfoService entryInfoService,
        ILoggerFactory loggerFactory,
        IPartialViewRenderService partialViewRenderHelper,
        IUnauthorizedImageService unauthorizedImageService) 
        : base(cookieService, creatorService)
    {
        _entryInfoService = entryInfoService;
        _loggerFactory = loggerFactory;
        _options = options.Value;
        _partialViewRenderHelper = partialViewRenderHelper;
        _unauthorizedImageService = unauthorizedImageService;
    }


    /// <summary>
    /// Adds one or more images to an entry.
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
    [HttpPost("entry-id/{entryId}")]
    [ProducesResponseType(typeof(ImageUploadResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequireCreatorCookie]
    [ValidateId("entryId")]
    public async Task<IActionResult> Add([FromRoute] string entryId, CancellationToken cancellationToken = default)
    {
        Debug.Assert(Request.ContentType is not null, "Content type is not null because of the [MultipartFormData] attribute");

        var decodedEntryId = IdCoder.Decode(entryId);

        var boundaryResult = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), _options.RequestBoundaryLengthLimit);
        if (boundaryResult.IsFailure)
            return BadRequest(boundaryResult.Error);

        var reader = new MultipartReader(boundaryResult.Value, HttpContext.Request.Body);
        if (reader is null)
            return BadRequest(DomainErrors.MultipartReaderError());

        var uploadResults = await ProcessUploadedFiles(reader, decodedEntryId, cancellationToken);

        return Ok(BuildUploadResults(uploadResults));
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
            return IdDecodingBadRequest();

        var decodedImageId = IdCoder.Decode(imageId);
        if (decodedImageId == Guid.Empty)
            return IdDecodingBadRequest();

        var (_, isFailure, value, error) = await _unauthorizedImageService.Get(decodedEntryId, decodedImageId, cancellationToken);
        if (isFailure)
            return BadRequest(error);

        return new FileStreamResult(value.Content, value.ContentType);
    }


    /// <summary>
    /// Returns the partial view HTML for a given image.
    /// </summary>
    /// <param name="entryId">Encoded identifier of the entry containing the image</param>
    /// <param name="imageId">Encoded identifier of the image to retrieve</param>
    /// <returns>
    /// Returns the rendered HTML of the partial view containing the image information or an internal server error if rendering fails.
    /// </returns>
    [HttpGet("entry-id/{entryId}/image-id/{imageId}/partial")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ValidateId("entryId", "imageId")]
    public async Task<IActionResult> GetImagePartial([FromRoute] string entryId, [FromRoute] string imageId, CancellationToken cancellationToken = default)
    {
        var decodedEntryId = IdCoder.Decode(entryId);
        var decodedImageId = IdCoder.Decode(imageId);

        var url = _unauthorizedImageService.BuildUrl(decodedEntryId, decodedImageId);
        var partialView = new PartialViewResult
        {
            ViewName = "Components/EditableImageContainer",
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = new EditableImageContainerModel(decodedImageId, url)
            }
        };

        var renderResult = await _partialViewRenderHelper.Render(ControllerContext, HttpContext, partialView);
        return Ok(renderResult);
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
    [HttpDelete("entry-id/{entryId}/image-id/{imageId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [RequireCreatorCookie]
    [ValidateId("entryId", "imageId")]
    public async Task<IActionResult> Remove([FromRoute] string entryId, [FromRoute] string imageId, CancellationToken cancellationToken = default)
    {
        var decodedEntryId = IdCoder.Decode(entryId);
        var decodedImageId = IdCoder.Decode(imageId);

        var creatorResult = await TryGetCreator(cancellationToken);
        if (creatorResult.IsFailure)
            return Forbid();

        await _entryInfoService.RemoveImage(creatorResult.Value, decodedEntryId, decodedImageId, cancellationToken);
        return NoContent();
    }



    private Dictionary<string, object> BuildUploadResults(List<Result<ImageUploadResult, DomainError>> uploadResults)
    {
        var results = new Dictionary<string, object>(uploadResults.Count);
        foreach (var (_, isFailure, imageUploadResult, error) in uploadResults)
        {
            if (isFailure)
            {
                var fileName = error.Extensions[ErrorExtensionKeys.FileName] as string;
                results.Add(fileName!, error);

                continue;
            }

            // TODO: remove the partialUrl from the response when the frontend is updated to stop using Razor components
            var partialUrl = _unauthorizedImageService.BuildPartialUrl(imageUploadResult.EntryId, imageUploadResult.Id);
            var url = _unauthorizedImageService.BuildUrl(imageUploadResult.EntryId, imageUploadResult.Id);

            results.Add(imageUploadResult.ClientFileName, imageUploadResult.ToImageUploadResponse(partialUrl, url));
        }

        return results;
    }


    private async Task<List<Result<ImageUploadResult, DomainError>>> ProcessUploadedFiles(MultipartReader reader, Guid decodedEntryId, CancellationToken cancellationToken)
    {
        var fileHelper = new FileHelper(_loggerFactory, _options.AllowedExtensions, _options.MaxFileSize);

        var uploadResults = new List<Result<ImageUploadResult, DomainError>>();
        // TODO: Consider using a configurable limit for the number of files
        // TODO: Use global limits for file count based by entry
        var uploadedFilesCount = 0;
        MultipartSection? section;

        do
        {
            if (_options.MaxFileCount <= uploadedFilesCount)
                break;

            section = await reader.ReadNextSectionAsync(cancellationToken);
            if (section is null)
                break;

            var (hasValue, value) = await ProcessSection(section, fileHelper, decodedEntryId, cancellationToken);
            if (!hasValue)
                continue;
            
            uploadResults.Add(value);
            uploadedFilesCount++;
        } while (section is not null);

        return uploadResults;
    }


    private async Task<(bool, Result<ImageUploadResult, DomainError>)> ProcessSection(MultipartSection section, FileHelper fileHelper, Guid decodedEntryId, CancellationToken cancellationToken)
    {
        if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
            return (false, default);

        if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
            return (false, default);

        var appFileResult = await fileHelper.ProcessStreamedFile(section, contentDisposition);
        if (appFileResult.IsFailure)
            return (true, EnrichDomainErrorWithFileName(contentDisposition, appFileResult.Error));

        var uploadResult = await _unauthorizedImageService.Add(decodedEntryId, appFileResult.Value, cancellationToken)
            .ToImageResponse(contentDisposition.FileName.Value)
            .OnFailureCompensate(x => EnrichDomainErrorWithFileName(contentDisposition, x));
        
        return (true, uploadResult);


        static DomainError EnrichDomainErrorWithFileName(ContentDispositionHeaderValue contentDisposition, DomainError error)
        {
            error.Extensions.Add(ErrorExtensionKeys.FileName, contentDisposition.FileName.Value);
            return error;
        }
    }


    private readonly IEntryInfoService _entryInfoService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ImageUploadOptions _options;
    private readonly IPartialViewRenderService _partialViewRenderHelper;
    private readonly IUnauthorizedImageService _unauthorizedImageService;
}
