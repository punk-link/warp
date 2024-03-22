using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Get([FromRoute] string entryId, [FromRoute] string imageId)
    {
        var decodedEntryId = IdCoder.Decode(entryId);
        if (decodedEntryId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var decodedImageId = IdCoder.Decode(imageId);
        if (decodedImageId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var (_, isFailure, value, error) = await _imageService.Get(decodedEntryId, decodedImageId);
        if (isFailure)
            return NotFound(error);

        return new FileStreamResult(new MemoryStream(value.Content), value.ContentType);
    }
    

    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] List<IFormFile> images)
    {
        var imageContainers = await _imageService.Add(images);
        var results = imageContainers.Select(x => new KeyValuePair<string, string>(x.Key, IdCoder.Encode(x.Value)))
            .ToList();

        return Ok(results);
    }

    
    private readonly IImageService _imageService;
}