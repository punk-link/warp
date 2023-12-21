using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Warp.WebApp.Extensions.Logging;
using Warp.WebApp.Helpers;

namespace Warp.WebApp.Pages;

public class BasePageModel : PageModel
{
    public BasePageModel(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<BasePageModel>();
    }
    
    
    protected void AddProblemDetails(ProblemDetails problemDetails)
    {
        var problemDetailsJson = JsonSerializer.Serialize(problemDetails);
        TempData[ProblemDetailsTempDataToken] = problemDetailsJson;
    }


    protected ProblemDetails? GetProblemDetails()
    {
        var problemDetailsObject = TempData[ProblemDetailsTempDataToken];
        if (problemDetailsObject is null)
            return default;
        
        var problemDetailsJson = (string) problemDetailsObject;
        return JsonSerializer.Deserialize<ProblemDetails>(problemDetailsJson);
    }


    protected IActionResult RedirectToError(ProblemDetails problemDetails)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        
        problemDetails = AddTracingData(problemDetails, traceId);
        AddProblemDetails(problemDetails);

        LogErrors(problemDetails, traceId);
        
        return RedirectToPage("./Error");
    }


    private ProblemDetails AddTracingData(ProblemDetails problemDetails, string traceId)
    {
        problemDetails.Instance = HttpContext.Request.Path;
        problemDetails.AddTraceId(traceId);
        
        return problemDetails;
    }


    private void LogErrors(ProblemDetails problemDetails, string traceId)
    {
        var errors = problemDetails.GetErrors();
        if (errors.Count == 0)
            _logger.LogGenericServerError(traceId);

        foreach (var error in errors)
            _logger.LogGenericServerError(traceId, $"{error.Code}: {error.Message}");
    }


    private const string ProblemDetailsTempDataToken = "ProblemDetails";

    private readonly ILogger<BasePageModel> _logger;
}