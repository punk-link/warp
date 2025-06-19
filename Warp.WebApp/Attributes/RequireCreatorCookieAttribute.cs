using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Filters;

namespace Warp.WebApp.Attributes;

/// <summary>
/// Ensures that the creator cookie is present for the action.
/// Returns Unauthorized if the cookie is missing.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireCreatorCookieAttribute : ServiceFilterAttribute
{
    public RequireCreatorCookieAttribute() : base(typeof(RequireCreatorCookieFilter))
    {
    }
}
