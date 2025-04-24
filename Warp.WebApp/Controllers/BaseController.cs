using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Errors;
using Warp.WebApp.Extensions;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Controllers;

public class BaseController : ControllerBase
{
    public BaseController(ICookieService cookieService, ICreatorService creatorService)
    {
        _cookieService = cookieService;
        _creatorService = creatorService;
    }


    public BadRequestObjectResult BadRequest(in DomainError error) 
        => base.BadRequest(ToProblemDetails(error));


    protected IActionResult BadRequestOrNoContent<T>(Result<T, DomainError> result)
    {
        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }


    protected async Task<Result<Creator, DomainError>> GetCreator(CancellationToken cancellationToken)
    {
        var creatorId = _cookieService.GetCreatorId(HttpContext);
        var (_, isFailure, creator, _) = await _creatorService.Get(creatorId, cancellationToken);
        if (isFailure)
            return DomainErrors.UnauthorizedError();
        
        return creator;
    }


    protected IActionResult IdDecodingBadRequest() 
        => BadRequest(DomainErrors.IdDecodingError());


    protected ProblemDetails ToProblemDetails(DomainError error)
    {
        var eventId = error.Code;
        var problemDetails = ProblemDetailsHelper.Create(eventId.ToDescriptionString(), error.Detail, eventId.ToHttpStatusCode())
            .AddEventId(eventId)
            .AddTraceId(HttpContext.TraceIdentifier);

        foreach (var extension in error.Extensions)
           problemDetails.Extensions[extension.Key] = extension.Value;

        return problemDetails;
    }


    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
}