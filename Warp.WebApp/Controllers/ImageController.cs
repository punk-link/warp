using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Services;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/images")]

public class ImageController : BaseController
{
    public ImageController(IImageService imageService)
    {
        _imageService = imageService;
    }


    [HttpGet("entry-id/{entryId:guid}/image-id/{imageId:guid}")]
    public async Task<IActionResult> Get([FromRoute] Guid entryId, [FromRoute] Guid imageId)
    {
        var (_, isFailure, value, error) = await _imageService.Get(entryId, imageId);
        if (isFailure)
            return NotFound(error);

        return new FileStreamResult(new MemoryStream(value.Content), value.ContentType);
    }
    

    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] List<IFormFile> images)
    {
        var results = await _imageService.Add(images);
        return Ok(results);
    }

    
    private readonly IImageService _imageService;
}