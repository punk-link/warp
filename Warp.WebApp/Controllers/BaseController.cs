using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Helpers;

namespace Warp.WebApp.Controllers;

public class BaseController : ControllerBase
{
    public BaseController(IStringLocalizer<ServerResources> localizer)
    {
        _localizer = localizer;
    }


    protected IActionResult NoContentOrBadRequest<T>(Result<T, ProblemDetails> result)
    {
        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }


    protected IActionResult ReturnIdDecodingBadRequest(string? detail = null)
    {
        detail ??= _localizer["IdDecodingErrorMessage"];
        return BadRequest(ProblemDetailsHelper.Create(detail));
    }


    private readonly IStringLocalizer<ServerResources> _localizer;
}