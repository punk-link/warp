using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Filters;

namespace Warp.WebApp.Attributes;

/// <summary>
/// Ensures that the ID parameter in the route can be decoded.
/// Returns a BadRequest if the ID cannot be decoded.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ValidateIdAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateIdAttribute"/> class with specified parameter names.
    /// </summary>
    /// <param name="parameterNames">Names of the route parameters to validate.</param>
    public ValidateIdAttribute(params string[] parameterNames)  : base(typeof(ValidateIdFilter))
    {
        Arguments = [parameterNames];
    }
}