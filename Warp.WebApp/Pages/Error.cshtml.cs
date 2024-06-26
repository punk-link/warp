using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models.ProblemDetails;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : BasePageModel
{
    public ErrorModel(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }


    public void OnGet([FromQuery] string? details)
    {
        var problemDetails = !string.IsNullOrWhiteSpace(details) 
            ? JsonSerializer.Deserialize<ProblemDetails>(details) 
            : GetProblemDetails();
        
        if (problemDetails is not null)
            FillModel(problemDetails);
    }


    public bool ShowErrors
        => Errors.Count > 0;


    public bool ShowRequestId
        => !string.IsNullOrEmpty(RequestId);


    private void FillModel(ProblemDetails problemDetails)
    {
        Detail = problemDetails.Detail;
        Errors = problemDetails.GetErrors();
        RequestId = problemDetails.GetTraceId();
        Title = problemDetails.Title;

        if (problemDetails.Status != null)
            Status = problemDetails.Status.Value;
    }


    public string? Detail { get; set; } = "An error occurred while processing your request.";
    public List<Error> Errors { get; set; } = Enumerable.Empty<Error>().ToList();
    public string? RequestId { get; set; }
    public int Status { get; set; } = 500;
    public string? Title { get; set; } = HttpStatusCode.InternalServerError.ToString();
}