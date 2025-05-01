using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models.ProblemDetails;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : BasePageModel
{
    public ErrorModel(ICookieService cookieService, 
        ICreatorService creatorService, 
        ILoggerFactory loggerFactory,
        IStringLocalizer<ServerResources> localizer) 
        : base(cookieService, creatorService, loggerFactory)
    {
        Detail = localizer["An error occurred while processing your request."];
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }


    public void OnGet([FromQuery] string? details)
    {
        var problemDetails = !string.IsNullOrWhiteSpace(details) 
            ? JsonSerializer.Deserialize<ProblemDetails>(details) 
            : GetProblemDetails();
        
        if (problemDetails is not null)
            BuildModel(problemDetails);
    }


    public bool ShowErrors
        => Errors.Count > 0;


    public bool ShowRequestId
        => !string.IsNullOrEmpty(RequestId);


    private void BuildModel(ProblemDetails problemDetails)
    {
        Detail = problemDetails.Detail;
        Errors = problemDetails.GetErrors();
        RequestId = problemDetails.GetTraceId();
        Title = problemDetails.Title;

        if (problemDetails.Status != null)
            Status = problemDetails.Status.Value;
    }


    public string? Detail { get; set; }
    public List<Error> Errors { get; set; } = Enumerable.Empty<Error>().ToList();
    public string? RequestId { get; set; }
    public int Status { get; set; } = 500;
    public string? Title { get; set; } = HttpStatusCode.InternalServerError.ToString();
}