using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Warp.WebApp.Models.Creators;

namespace Warp.WebApp.Services.Creators;

public class CookieService : ICookieService
{
    public Guid? GetCreatorId(HttpContext httpContext)
    {
        var claim = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
        if (claim is null)
            return null;

        if (!Guid.TryParse(claim.Value, out var foundCreatorId))
            return null;

        return foundCreatorId;
    }


    public async Task Set(HttpContext httpContext, Creator creator)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, creator.Id.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, _defaultAuthenticationProperties);
    }


    private static readonly AuthenticationProperties _defaultAuthenticationProperties = new()
    {
        IsPersistent = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(365)
    };
}