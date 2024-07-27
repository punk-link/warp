using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Helpers;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/images")]
public sealed class ImageController : BaseController
{
    public ImageController(IStringLocalizer<ServerResources> localizer, IImageService imageService, PartialViewRenderHelper partialViewRenderHelper) 
        : base(localizer)
    {
        _imageService = imageService;
        _partialViewRenderHelper = partialViewRenderHelper;
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

        var (_, isFailure, value, error) = await _imageService.Get(decodedEntryId, decodedImageId, cancellationToken);
        if (isFailure)
            return NotFound(error);

        return new FileStreamResult(new MemoryStream(value.Content), value.ContentType);
    }


    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] List<IFormFile> images, CancellationToken cancellationToken = default)
    {
        var imageContainers = (await _imageService.Add(images, cancellationToken))
            .Select(x => new KeyValuePair<string, string>(x.Key, IdCoder.Encode(x.Value)))
            .ToList();

        return Ok(await BuildUploadResults(imageContainers));
    }


    private async Task<List<string>> BuildUploadResults(List<KeyValuePair<string, string>> imageContainers)
    {
        var uploadResults = new List<string>(imageContainers.Count);

        foreach (var container in imageContainers)
        {
            var partialView = new PartialViewResult
            {
                ViewName = "Components/ImageContainer",
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = new ImageContainerModel(container.Value, isEditable: true)
                }
            };

            var html = await _partialViewRenderHelper.ToString(ControllerContext, HttpContext, partialView);
            uploadResults.Add(html);
        }

        return uploadResults;
    }


    private readonly IImageService _imageService;
    private readonly PartialViewRenderHelper _partialViewRenderHelper;
}