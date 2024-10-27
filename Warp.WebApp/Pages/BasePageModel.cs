using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Warp.WebApp.Telemetry.Logging;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Services.Creators;
using CSharpFunctionalExtensions;
using Warp.WebApp.Services;
using Microsoft.Extensions.Localization;

namespace Warp.WebApp.Pages;

public class BasePageModel : PageModel
{
    public BasePageModel(ICookieService cookieService, 
        ICreatorService creatorService, 
        ILoggerFactory loggerFactory, 
        IStringLocalizer<ServerResources> serverLocalizer)
    {
        _cookieService = cookieService;
        _creatorService = creatorService;
        _logger = loggerFactory.CreateLogger<BasePageModel>();
        _serverLocalizer = serverLocalizer;
    }


    protected void AddProblemDetails(ProblemDetails problemDetails)
    {
        var problemDetailsJson = JsonSerializer.Serialize(problemDetails);
        TempData[ProblemDetailsTempDataToken] = problemDetailsJson;
    }


    protected Result<Guid, ProblemDetails> DecodeId(string id)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return ProblemDetailsHelper.Create(_serverLocalizer["IdDecodingErrorMessage"]);

        return decodedId;
    }


    protected async Task<Result<Creator, ProblemDetails>> GetCreator(CancellationToken cancellationToken)
    {
        var creatorId = _cookieService.GetCreatorId(HttpContext);
        return await _creatorService.Get(creatorId, cancellationToken);
    }


    protected ProblemDetails? GetProblemDetails()
    {
        var problemDetailsObject = TempData[ProblemDetailsTempDataToken];
        if (problemDetailsObject is null)
            return default;

        var problemDetailsJson = (string)problemDetailsObject;
        return JsonSerializer.Deserialize<ProblemDetails>(problemDetailsJson);
    }


    protected IActionResult RedirectToError(ProblemDetails problemDetails)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        problemDetails = AddTracingData(problemDetails, traceId);
        AddProblemDetails(problemDetails);

        if (problemDetails.Status != (int)HttpStatusCode.NotFound)
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

    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
    private readonly ILogger<BasePageModel> _logger;
    private readonly IStringLocalizer<ServerResources> _serverLocalizer;
}