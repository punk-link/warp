using Microsoft.AspNetCore.Mvc;

namespace Warp.WebApp.Services.Infrastructure;

public interface IPartialViewRenderService
{
    Task<string> Render(ControllerContext controllerContext, HttpContext httpContext, PartialViewResult partialView);
}