using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Warp.WebApp.Extensions.Logging;

namespace Warp.WebApp.Helpers;

public class PartialViewRenderHelper
{
    public PartialViewRenderHelper(ILoggerFactory loggerFactory, ITempDataDictionaryFactory tempDataDictionaryFactory, ICompositeViewEngine viewEngine)
    {
        _logger = loggerFactory.CreateLogger<PartialViewRenderHelper>();

        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _viewEngine = viewEngine;
    }


    public async Task<string> Render(ControllerContext controllerContext, HttpContext httpContext, PartialViewResult partialView)
    {
        var viewResult = GetViewEngineResult(controllerContext, partialView.ViewName!);

        try
        {
            await using var writer = new StringWriter();
            var tempDataDictionary = _tempDataDictionaryFactory.GetTempData(httpContext);
            var viewContext = new ViewContext(controllerContext, viewResult.View!, partialView.ViewData, tempDataDictionary, writer, new HtmlHelperOptions());

            await viewResult.View!.RenderAsync(viewContext);

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
        try
        {
            return _viewEngine.FindView(controllerContext, viewName, false);
        }
        catch (Exception ex)
        {
            _logger.LogPartialViewNotFound(viewName, ex.Message);
            throw;
        }
    }


    private readonly ILogger<PartialViewRenderHelper> _logger;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly ICompositeViewEngine _viewEngine;
}
