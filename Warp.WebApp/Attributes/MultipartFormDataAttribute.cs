using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Warp.WebApp.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class MultipartFormDataAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;
        if (request.HasFormContentType && request.ContentType!.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            return;

        context.Result = new UnsupportedMediaTypeResult();
    }
}
