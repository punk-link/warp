using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.Infrastructure;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/images")]
public sealed class ImageController : BaseController
{
    public ImageController(IStringLocalizer<ServerResources> localizer, IImageService imageService, IPartialViewRenderService partialViewRenderHelper) 
        : base(localizer)
    {
        _imageService = imageService;
        _partialViewRenderHelper = partialViewRenderHelper;
    }


    [HttpGet("entry-id/{entryId}/image-id/{imageId}")]
    [OutputCache(Duration = 10 * 60, VaryByRouteValueNames = ["entryId", "imageId"])]
    public async Task<IActionResult> Get([FromRoute] string imageId, CancellationToken cancellationToken = default)
    {
        var decodedImageId = IdCoder.Decode(imageId);
        if (decodedImageId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var (_, isFailure, value, error) = await _imageService.Get(decodedImageId, cancellationToken);
        if (isFailure)
            return NotFound(error);

        return new FileStreamResult(new MemoryStream(value.Content), value.ContentType);
    }


    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] List<IFormFile> images, CancellationToken cancellationToken = default)
    {
        var imageContainers = await _imageService.Add(images, cancellationToken);
        return Ok(await BuildUploadResults(imageContainers));
    }


    private async Task<Dictionary<string, string>> BuildUploadResults(Dictionary<string, Guid> imageContainers)
    {
        var uploadResults = new Dictionary<string, string>(imageContainers.Count);

        foreach (var container in imageContainers)
        {
            var partialView = new PartialViewResult
            {
                
                ViewName = "Components/EditableImageContainer",
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = new EditableImageContainerModel(container.Value, null)
                }
            };

            var html = await _partialViewRenderHelper.Render(ControllerContext, HttpContext, partialView);
            uploadResults.Add(container.Key, html);
        }

        return uploadResults;
    }


    private readonly IImageService _imageService;
    private readonly IPartialViewRenderService _partialViewRenderHelper;
}