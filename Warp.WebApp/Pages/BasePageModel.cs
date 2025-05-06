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
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Extensions;

namespace Warp.WebApp.Pages;

public class BasePageModel : PageModel
{
    public BasePageModel(ICookieService cookieService, 
        ICreatorService creatorService, 
        ILoggerFactory loggerFactory)
    {
        _cookieService = cookieService;
        _creatorService = creatorService;
        _logger = loggerFactory.CreateLogger<BasePageModel>();
    }


    protected void AddProblemDetails(ProblemDetails problemDetails)
    {
        var problemDetailsJson = JsonSerializer.Serialize(problemDetails);
        TempData[ProblemDetailsTempDataToken] = problemDetailsJson;
    }


    protected Result<Guid, DomainError> DecodeId(string id)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return DomainErrors.IdDecodingError();

        return decodedId;
    }


    protected async Task<Result<Creator, DomainError>> GetCreator(CancellationToken cancellationToken)
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


    protected IActionResult RedirectToError(in DomainError error)
    {
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        error.AddTraceId(traceId);

        var problemDetails = error.ToProblemDetails();
        problemDetails.Instance = HttpContext.Request.Path;
        if (IsSkipableError(error))
            LogErrors(problemDetails);

        AddProblemDetails(problemDetails);

        return RedirectToPage("./Error");


        bool IsSkipableError(DomainError error) 
            => error.Code.ToHttpStatusCode() == HttpStatusCode.NotFound;


        void LogErrors(ProblemDetails problemDetails)
        {
            // We're logging the problem details here, but ideally we should log the domain error
            // let's keep it that way until we move out from razor pages
            var errors = problemDetails.GetErrors();
            if (errors.Count == 0)
                _logger.LogServerError(traceId);

            foreach (var error in errors)
                _logger.LogServerErrorWithMessage(traceId, $"{error.Code}: {error.Message}");
        }
    }


    private const string ProblemDetailsTempDataToken = "ProblemDetails";

    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
    private readonly ILogger<BasePageModel> _logger;
}