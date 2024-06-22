using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Warp.WebApp.Services.Creators;

public class CookieService : ICookieService
{
    public async Task<Guid> ConfigureCookie(HttpContext httpContext, HttpResponse response)
    {

        if (httpContext.User.Claims.Any())
        {
            var existingClaims = httpContext.User.Claims.ToList();
            if (Guid.TryParse(existingClaims.First(x => x.Type == ClaimTypes.Name).Value, out var foundUserId))
                return foundUserId;
        }

        var userId = Guid.NewGuid();
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Name, userId.ToString())
        ];
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        await response.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
        });

        return userId;
    }


    public static Claim? GetClaim(HttpContext httpContext)
    {
        var claim = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name && Guid.TryParse(x.Value, out _));
        return claim;
    }
}