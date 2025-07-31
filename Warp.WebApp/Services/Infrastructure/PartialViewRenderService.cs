using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Services.Infrastructure;

public class PartialViewRenderService : IPartialViewRenderService
{
    public PartialViewRenderService(ILoggerFactory loggerFactory, ITempDataDictionaryFactory tempDataDictionaryFactory, ICompositeViewEngine viewEngine)
    {
        _logger = loggerFactory.CreateLogger<PartialViewRenderService>();

        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _viewEngine = viewEngine;
    }


    public async Task<string> Render(ControllerContext controllerContext, HttpContext httpContext, PartialViewResult partialView)
    {
        var viewEngineResult = GetViewEngineResult(controllerContext, partialView.ViewName!);

        try
        {
            await using var writer = new StringWriter();
            var tempDataDictionary = _tempDataDictionaryFactory.GetTempData(httpContext);
            var viewContext = new ViewContext(controllerContext, viewEngineResult.View!, partialView.ViewData, tempDataDictionary, writer, new HtmlHelperOptions());

            await viewEngineResult.View!.RenderAsync(viewContext);

            return writer.GetStringBuilder().ToString();
        }
        catch (Exception ex)
        {
            _logger.LogPartialViewRenderingError(partialView.ViewName!, ex.Message);
            throw;
        }
    }


    private ViewEngineResult GetViewEngineResult(ControllerContext controllerContext, string viewName)
    {
        var viewResult = _viewEngine.FindView(controllerContext, viewName, false);
        if (viewResult.Success)
            return viewResult;

        var ex = new InvalidOperationException($"Partial view '{viewName}' not found.");
        _logger.LogPartialViewNotFound(viewName, ex.Message);

        throw ex;
    }


    private readonly ILogger<PartialViewRenderService> _logger;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly ICompositeViewEngine _viewEngine;
}
