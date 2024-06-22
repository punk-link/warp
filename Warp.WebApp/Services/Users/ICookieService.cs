namespace Warp.WebApp.Services.Users;

public interface ICookieService
{
    public Task<Guid> ConfigureCookie(HttpContext httpContext, HttpResponse response);
}
