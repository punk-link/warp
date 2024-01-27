using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace Warp.WebApp.Controllers;

public class BaseController : ControllerBase
{
    protected IActionResult NoContentOrBadRequest<T>(Result<T, ProblemDetails> result)
    {
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return NoContent();
    }
}