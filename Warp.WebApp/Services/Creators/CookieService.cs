using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Warp.WebApp.Models.Creators;

namespace Warp.WebApp.Services.Creators;

/// <summary>
/// Implements functionality for managing creator authentication cookies.
/// </summary>
public class CookieService : ICookieService
{
    /// <inheritdoc cref="ICookieService.GetCreatorId"/>/>
    public Guid? GetCreatorId(HttpContext httpContext)
    {
        var claim = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
        if (claim is null)
            return null;

        if (!Guid.TryParse(claim.Value, out var foundCreatorId))
            return null;

        return foundCreatorId;
    }


    /// <inheritdoc cref="ICookieService.Set"/>
    public async Task Set(HttpContext httpContext, Creator creator)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, creator.Id.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var authenticationProperties = new AuthenticationProperties
        {
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(365),
            IsPersistent = true,
            IssuedUtc = DateTimeOffset.UtcNow
        };

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authenticationProperties);

        httpContext.User = claimsPrincipal;
    }
}