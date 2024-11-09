﻿using CSharpFunctionalExtensions;
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
        IImageService imageService, 
        IStringLocalizer<ServerResources> localizer,
        IPartialViewRenderService partialViewRenderHelper) 
        : base(localizer, cookieService, creatorService)
    {
        _imageService = imageService;
        _partialViewRenderHelper = partialViewRenderHelper;
    }


    [HttpGet("entry-id/{entryId}/image-id/{imageId}")]
    [OutputCache(Duration = 10 * 60, VaryByRouteValueNames = ["entryId", "imageId"])]
    public async Task<IActionResult> Get([FromRoute] string imageId, CancellationToken cancellationToken = default)
    {
        // TODO: add a check for the entryId

        var decodedImageId = IdCoder.Decode(imageId);
        if (decodedImageId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var (_, isFailure, value, error) = await _imageService.Get(decodedImageId, cancellationToken);
        if (isFailure)
            return NotFound(error);

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

        await _imageService.Remove(decodedImageId, cancellationToken);
        return NoContent();
    }


    [HttpPost("entry-id/{entryId}")]
    public async Task<IActionResult> Upload([FromRoute] string entryId, [FromForm] List<IFormFile> images, CancellationToken cancellationToken = default)
    {
        var decodedEntryId = IdCoder.Decode(entryId);
        if (decodedEntryId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var imageResponses = await _imageService.Add(decodedEntryId, images, cancellationToken);
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


    private readonly IImageService _imageService;
    private readonly IPartialViewRenderService _partialViewRenderHelper;
}