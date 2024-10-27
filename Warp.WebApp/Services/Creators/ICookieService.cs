using Warp.WebApp.Models.Creators;

namespace Warp.WebApp.Services.Creators;

public interface ICookieService
{
    public Guid? GetCreatorId(HttpContext httpContext);
    public Task Set(HttpContext httpContext, Creator creator);
}
