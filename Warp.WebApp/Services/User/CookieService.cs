using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Sentry.Protocol;
using System.Security.Claims;
using CSharpFunctionalExtensions;

namespace Warp.WebApp.Services.User;

public class CookieService : ICookieService
{
    public async Task<Guid> ConfigureCookie(HttpContext httpContext, HttpResponse response)
    {
        List<Claim> claims = new List<Claim>();
        Claim claim;

        if (httpContext.User.Claims != null && httpContext.User.Claims.Any())
        {
            claims = httpContext.User.Claims.ToList();
            Guid.TryParse(claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value, out var foundUserId);
            return foundUserId;
        }
        else
            claim = new Claim(ClaimTypes.Name, Guid.NewGuid().ToString());

        Guid.TryParse(claim!.Value, out var userId);
        claims.Add(claim);
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        await response.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
        });

        return userId;
    }
}
