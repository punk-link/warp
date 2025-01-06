using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
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

[ApiController]
[Route("/api/images")]
public sealed class ImageController : BaseController
{
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

        var (_, isFailure, boundary, error) = MultipartRequestHelper.GetBoundary(_localizer, MediaTypeHeaderValue.Parse(Request.ContentType), _options.RequestBoundaryLengthLimit);
        if (isFailure)
            return BadRequest(error);

        var reader = new MultipartReader(boundary, HttpContext.Request.Body);
        if (reader is null)
            return BadRequest(ProblemDetailsHelper.Create(_localizer["Failed to create MultipartReader"]));

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

            if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                continue;

            if (!MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                continue;

            var (_, isAppFileFailure, appFile, appFileError) = await fileHelper.ProcessStreamedFile(section, contentDisposition);
            if (isAppFileFailure)
            {
                uploadResults.Add(EnrichProblemDetails(contentDisposition, appFileError));
                continue;
            }

            var uploadResult = await _unauthorizedImageService.Add(decodedEntryId, appFile, cancellationToken);
            uploadResults.Add(uploadResult);

            uploadedFilesCount++;
        } while (section is not null);

        return Ok(await BuildUploadResults(uploadResults));


        static ProblemDetails EnrichProblemDetails(ContentDispositionHeaderValue contentDisposition, ProblemDetails problemDetails)
        {
            problemDetails.Extensions.Add(ProblemDetailsFileNameExtensionKey, contentDisposition.FileName.Value);
            return problemDetails;
        }
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


    private const string ProblemDetailsFileNameExtensionKey = "fileName";

    private readonly IEntryInfoService _entryInfoService;
    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ImageUploadOptions _options;
    private readonly IPartialViewRenderService _partialViewRenderHelper;
    private readonly IUnauthorizedImageService _unauthorizedImageService;
}
