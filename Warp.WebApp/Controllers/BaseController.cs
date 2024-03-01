using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Helpers;

namespace Warp.WebApp.Controllers;

public class BaseController : ControllerBase
{
    protected IActionResult NoContentOrBadRequest<T>(Result<T, ProblemDetails> result)
    {
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return NoContent();
    }


    protected IActionResult ReturnIdDecodingBadRequest(string detail = "Can't decode a provided ID.")
        => BadRequest(ProblemDetailsHelper.Create(detail));
}