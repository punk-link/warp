namespace Warp.WebApp.Services.Creators;

public interface ICookieService
{
    public Guid? GetCreatorId(HttpContext httpContext);
    public Task Set(HttpContext httpContext, Guid creatorId);
}
