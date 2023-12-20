using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : BasePageModel
{
    public string? RequestId { get; set; }


    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }


    public void OnGet()
    {
        var problemDetails = TempData["ProblemDetails"];
        if (problemDetails is not null)
        { }

        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }


    public bool ShowRequestId 
        => !string.IsNullOrEmpty(RequestId);

    
    private readonly ILogger<ErrorModel> _logger;
}