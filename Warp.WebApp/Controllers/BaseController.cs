using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Controllers;

public abstract class BaseController : ControllerBase
{
    public BaseController(ICookieService cookieService, ICreatorService creatorService)
    {
        _cookieService = cookieService;
        _creatorService = creatorService;
    }


    protected BadRequestObjectResult BadRequest(in DomainError error) 
        => base.BadRequest(error.ToProblemDetails());


    protected IActionResult OkOrBadRequest<T>(Result<T, DomainError> result)
    {
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Ok(result.Value);
    }


    protected async Task<Creator> GetCreator(CancellationToken cancellationToken)
    {
        var creatorId = _cookieService.GetCreatorId(HttpContext);
        var (_, isFailure, creator, _) = await _creatorService.Get(creatorId, cancellationToken);
        if (isFailure)
            throw new UnauthorizedAccessException("Unauthorized access to creator.");

        return creator;
    }


    protected async Task<Result<Creator, DomainError>> TryGetCreator(CancellationToken cancellationToken)
    {
        var creatorId = _cookieService.GetCreatorId(HttpContext);
        var (_, isFailure, creator, _) = await _creatorService.Get(creatorId, cancellationToken);
        if (isFailure)
            return DomainErrors.UnauthorizedError();
        
        return creator;
    }


    protected IActionResult IdDecodingBadRequest() 
        => BadRequest(DomainErrors.IdDecodingError());


    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
}