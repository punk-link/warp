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
    

    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] List<IFormFile> images)
    {
        var results = await _imageService.Add(images);
        return Ok(results);
    }

    
    private readonly IImageService _imageService;
}