using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Services;

namespace Warp.WebApp.Filters;

/// <summary>
/// Ensures that the ID parameter in the route can be decoded.
/// Returns a BadRequest if the ID cannot be decoded.
/// </summary>
public class ValidateIdFilter : IActionFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateIdFilter"/> class with specified parameter names.
    /// </summary>
    /// <param name="parameterNames">Names of the route parameters to validate.</param>
    public ValidateIdFilter(string[] parameterNames)
    {
        _parameterNames = parameterNames;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateIdFilter"/> class with default parameter name "id".
    /// </summary>
    public ValidateIdFilter() : this(["id"])
    {
    }


    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var paramName in _parameterNames)
        {
            var error = DomainErrors.IdDecodingError();
            if (!context.RouteData.Values.TryGetValue(paramName, out var idValue) || idValue is null)
            {
                context.Result = new BadRequestObjectResult(error.ToProblemDetails());
                return;
            }

            string? id = idValue.ToString();
            var decodedId = IdCoder.Decode(id);
            if (decodedId == Guid.Empty)
            {
                context.Result = new BadRequestObjectResult(error.ToProblemDetails());
                break;
            }
        }
    }


    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }


    private readonly string[] _parameterNames;
}