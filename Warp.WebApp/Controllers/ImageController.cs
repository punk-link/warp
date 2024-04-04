using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/images")]
public sealed class ImageController : BaseController
{
    public ImageController(IImageService imageService)
    {
        _imageService = imageService;
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
        var imageContainers = await _imageService.Add(images, cancellationToken);
        var results = imageContainers.Select(x => new KeyValuePair<string, string>(x.Key, IdCoder.Encode(x.Value)))
            .ToList();

        return Ok(results);
    }

    
    private readonly IImageService _imageService;
}