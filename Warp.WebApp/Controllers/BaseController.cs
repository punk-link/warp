using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Net;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Controllers;

public class BaseController : ControllerBase
{
    public BaseController(IStringLocalizer<ServerResources> localizer, ICookieService cookieService, ICreatorService creatorService)
    {
        _cookieService = cookieService;
        _creatorService = creatorService;
        _localizer = localizer;
    }


    protected async Task<Result<Creator, ProblemDetails>> GetCreator(CancellationToken cancellationToken)
    {
        var creatorId = _cookieService.GetCreatorId(HttpContext);
        var (_, isFailure, creator, _) = await _creatorService.Get(creatorId, cancellationToken);
        if (isFailure)
            return ProblemDetailsHelper.CreateUnauthorized(_localizer);

        return creator;
    }


    protected IActionResult NoContentOrBadRequest<T>(Result<T, ProblemDetails> result)
    {
        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }


    protected IActionResult ReturnForbid()
    {
        return StatusCode((int)HttpStatusCode.Forbidden, ProblemDetailsHelper.CreateForbidden(_localizer));
    }


    protected IActionResult ReturnIdDecodingBadRequest(string? detail = null)
    {
        detail ??= _localizer["IdDecodingErrorMessage"];
        return BadRequest(ProblemDetailsHelper.Create(detail));
    }


    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
    private readonly IStringLocalizer<ServerResources> _localizer;
}