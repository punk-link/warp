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


    protected IActionResult OkOrBadRequest(UnitResult<DomainError> result)
    {
        if (result.IsFailure)
            return BadRequest(result.Error);
        
        return Ok(true);
    }


    protected async Task<Creator> GetCreator(CancellationToken cancellationToken)
    {
        var (_, isFailure, creator, _) = await TryGetCreatorInternal(cancellationToken);
        if (isFailure)
            throw new UnauthorizedAccessException("Unauthorized access to creator.");

        return creator;
    }


    protected async Task<Creator> GetCreatorOrDefault(CancellationToken cancellationToken)
    {
        var (_, isFailure, creator, _) = await TryGetCreatorInternal(cancellationToken);
        if (isFailure)
            return default;

        return creator;
    }


    protected async Task<Result<Creator, DomainError>> TryGetCreator(CancellationToken cancellationToken)
    {
        var (_, isFailure, creator, _) = await TryGetCreatorInternal(cancellationToken);
        if (isFailure)
            return DomainErrors.UnauthorizedError();
        
        return creator;
    }


    protected IActionResult IdDecodingBadRequest() 
        => BadRequest(DomainErrors.IdDecodingError());


    private Task<Result<Creator, DomainError>> TryGetCreatorInternal(CancellationToken cancellationToken)
    {
        var creatorId = _cookieService.GetCreatorId(HttpContext);
        return _creatorService.Get(creatorId, cancellationToken);
    }


    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
}