namespace Warp.WebApp.Services.User;

public interface ICookieService
{
    public Task<Guid> ConfigureCookie(HttpContext httpContext, HttpResponse response);
}
