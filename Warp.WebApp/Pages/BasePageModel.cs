using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Warp.WebApp.Helpers;

namespace Warp.WebApp.Pages;

public class BasePageModel : PageModel
{
    protected void AddProblemDetails(ProblemDetails problemDetails)
    {
        var problemDetailsJson = JsonSerializer.Serialize(problemDetails);
        TempData[ProblemDetailsTempDataToken] = problemDetailsJson;
    }


    protected ProblemDetails? AddProblemDetails()
    {
        var problemDetailsObject = TempData[ProblemDetailsTempDataToken];
        if (problemDetailsObject is null)
            return default;
        
        var problemDetailsJson = (string) problemDetailsObject;
        return JsonSerializer.Deserialize<ProblemDetails>(problemDetailsJson);
    }


    protected IActionResult RedirectToError(ProblemDetails problemDetails)
    {
        problemDetails = AddTracingData(problemDetails);
        AddProblemDetails(problemDetails);
        
        //_logger.LogError(error!);
        
        return RedirectToPage("./Error");
    }


    private ProblemDetails AddTracingData(ProblemDetails problemDetails)
    {
        problemDetails.Instance = HttpContext.Request.Path;
        
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        problemDetails.AddTraceId(traceId);
        
        return problemDetails;
    }


    private const string ProblemDetailsTempDataToken = "ProblemDetails";
}