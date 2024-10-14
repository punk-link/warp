using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Warp.WebApp.Controllers;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Services.Infrastructure;

public class UrlService : IUrlService
{
    public UrlService(ILoggerFactory loggerFactory, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
    {
        _logger = loggerFactory.CreateLogger<UrlService>();

        _urlHelperFactory = urlHelperFactory;
        _actionContextAccessor = actionContextAccessor;
    }


    public Uri GetImageUrl(string decodedEntryId, in Guid imageId)
        => GetImageUrl(IdCoder.Decode(decodedEntryId), imageId);


    public Uri GetImageUrl(in Guid entryId, in Guid imageId)
    {
        var actionContext = _actionContextAccessor.ActionContext;
        if (actionContext is null)
            _logger.LogActionContextNotFound();

        var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext!);
        var controllerName = nameof(ImageController).Replace("Controller", string.Empty);

        var values = new
        {
            entryId = IdCoder.Encode(entryId),
            imageId = IdCoder.Encode(imageId)
        };

        var url = urlHelper.Action(nameof(ImageController.Get), controllerName, values);
        if (url is null)
            _logger.LogImageControllerGetMethodNotFound();

        return new Uri(url!, UriKind.Relative);
    }


    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly ILogger<UrlService> _logger;
    private readonly IUrlHelperFactory _urlHelperFactory;
}
