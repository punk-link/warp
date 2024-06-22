namespace Warp.WebApp.Services.Creators;

public interface ICookieService
{
    public Task<Guid> ConfigureCookie(HttpContext httpContext, HttpResponse response);
}
