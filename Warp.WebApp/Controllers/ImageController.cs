using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Models;
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
    public ImageController(ICookieService cookieService, 
        ICreatorService creatorService,
        IEntryInfoService entryInfoService,
        IPartialViewRenderService partialViewRenderHelper,
        IUnauthorizedImageService unauthorizedImageService,
        IStringLocalizer<ServerResources> localizer) 
        : base(localizer, cookieService, creatorService)
    {
        _entryInfoService = entryInfoService;
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


    [HttpPost("entry-id/{entryId}")]
    public async Task<IActionResult> Upload([FromRoute] string entryId, [FromForm] List<IFormFile> images, CancellationToken cancellationToken = default)
    {
        var decodedEntryId = IdCoder.Decode(entryId);
        if (decodedEntryId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var imageResponses = await _unauthorizedImageService.Add(decodedEntryId, images, cancellationToken);
        return Ok(await BuildUploadResults(imageResponses));
    }


    private async Task<Dictionary<string, string>> BuildUploadResults(List<ImageResponse> imageResponses)
    {
        var uploadResults = new Dictionary<string, string>(imageResponses.Count);

        foreach (var imageResponse in imageResponses)
        {
            var partialView = new PartialViewResult
            {
                
                ViewName = "Components/EditableImageContainer",
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = new EditableImageContainerModel(imageResponse.ImageInfo)
                }
            };

            var html = await _partialViewRenderHelper.Render(ControllerContext, HttpContext, partialView);
            uploadResults.Add(imageResponse.ClientFileName, html);
        }

        return uploadResults;
    }


    private readonly IEntryInfoService _entryInfoService;
    private readonly IPartialViewRenderService _partialViewRenderHelper;
    private readonly IUnauthorizedImageService _unauthorizedImageService;
}