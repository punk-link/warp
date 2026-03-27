using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Attributes;

/// <summary>
/// Validates the CSRF token supplied in the <c>X-CSRF-TOKEN</c> request header against the antiforgery cookie.
/// Returns 400 Bad Request when validation fails.
/// </summary>
/// <remarks>
/// Use this instead of <see cref="Microsoft.AspNetCore.Mvc.ValidateAntiForgeryTokenAttribute"/> on API
/// controllers. This project does not use MVC views — it registers controllers via <c>AddControllers()</c>,
/// not <c>AddControllersWithViews()</c> — so the built-in attribute's dependency on view-feature services
/// is an unwanted coupling. This attribute validates antiforgery tokens directly via <see cref="IAntiforgery"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ValidateCsrfTokenAttribute : Attribute, IAsyncActionFilter
{
    /// <inheritdoc/>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();

        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            var result = DomainErrors.CsrfTokenValidationFailure()
                .ToProblemDetails();

            context.Result = new ObjectResult(result)
            {
                StatusCode = result.Status,
                ContentTypes = { "application/problem+json" }
            };

            return;
        }

        await next();
    }
}
