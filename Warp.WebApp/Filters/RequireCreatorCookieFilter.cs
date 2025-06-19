using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Filters;

/// <summary>
/// Ensures that the creator cookie is present for the action.
/// Returns Unauthorized if the cookie is missing.
/// </summary>
public class RequireCreatorCookieFilter : IActionFilter
{
    public RequireCreatorCookieFilter(ICookieService cookieService)
    {
        _cookieService = cookieService;
    }
    

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var creatorId = _cookieService.GetCreatorId(context.HttpContext);
        if (creatorId is null)
        {
            var unauthorizedError = DomainErrors.UnauthorizedError();
            context.Result = new UnauthorizedObjectResult(unauthorizedError.ToProblemDetails());
        }
    }

    
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }

    
    private readonly ICookieService _cookieService;
}
