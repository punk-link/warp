using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class IndexModel : BasePageModel
{
    public IndexModel(IOptionsSnapshot<AnalyticsOptions> analyticsOptions, ILoggerFactory loggerFactory, IStringLocalizer<IndexModel> localizer,
        IStringLocalizer<ServerResources> serverLocalizer, IEntryService entryService, ICookieService cookieService, ICreatorService creatorService)
        : base(loggerFactory)
    {
        _analyticsOptions = analyticsOptions.Value;
        _cookieService = cookieService;
        _creatorService = creatorService;
        _entryService = entryService;
        _localizer = localizer;
        _serverLocalizer = serverLocalizer;
    }


    [OutputCache(Duration = 3600)]
    public async Task<IActionResult> OnGet(string? id, CancellationToken cancellationToken)
    {
        AnalyticsModel = new AnalyticsModel(_analyticsOptions);

        if (string.IsNullOrEmpty(id))
        {
            OpenGraphModel = OpenGraphService.GetDefaultModel(_serverLocalizer["DefaultOpenGraphDescriptionText"]);
            return Page();
        }

        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return RedirectToError(ProblemDetailsHelper.Create("Can't decode a provided ID."));

        var (_, isFailure, entry, problemDetails) = await _entryService.Get(decodedId, cancellationToken);
        if (isFailure)
            return RedirectToError(problemDetails);

        BuildModel(entry);
        OpenGraphModel = OpenGraphService.GetModel(TextContent);
        return Page();


        void BuildModel(EntryInfo entryInfo)
        {
            TextContent = TextFormatter.GetCleanString(entryInfo.Entry.Content);
            ImageIds = entryInfo.ImageIds;
            SelectedExpirationPeriod = GetExpirationPeriodId(entryInfo.Entry.ExpiresAt - entryInfo.Entry.CreatedAt);
        }
    }


    public async Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        var expiresIn = GetExpirationPeriod(SelectedExpirationPeriod);
        var creatorId = _cookieService.GetCreatorId(HttpContext);
        var (_, isCreationFailed, creator, creatorError) = await _creatorService.GetOrAdd(creatorId, cancellationToken);
        if (isCreationFailed)
            return RedirectToError(creatorError);

        var (_, isFailure, entry, entryError) = await _entryService.Add(TextContent, expiresIn, ImageIds, cancellationToken);
        if (isFailure)
            return RedirectToError(entryError);

        var attachResult = await _creatorService.AttachEntry(creator, entry, expiresIn, cancellationToken);
        if (attachResult.IsFailure)
            return RedirectToError(attachResult.Error);
        
        await _cookieService.Set(HttpContext, creator.Id);

        return RedirectToPage("./Preview", new { id = IdCoder.Encode(entry.Id) });
    }


    private static TimeSpan GetExpirationPeriod(int selectedPeriod)
        => selectedPeriod switch
        {
            1 => new TimeSpan(0, 5, 0),
            2 => new TimeSpan(0, 30, 0),
            3 => new TimeSpan(1, 0, 0),
            4 => new TimeSpan(8, 0, 0),
            5 => new TimeSpan(24, 0, 0),
            _ => new TimeSpan(0, 5, 0)
        };


    private static int GetExpirationPeriodId(TimeSpan expirationPeriod)
    {
        var ts5Min = new TimeSpan(0, 5, 0);
        var ts30Min = new TimeSpan(0, 30, 0);
        var ts1Hour = new TimeSpan(1, 0, 0);
        var ts8Hours = new TimeSpan(8, 0, 0);

        if (expirationPeriod <= ts5Min)
            return 1;
        if (expirationPeriod <= ts30Min && expirationPeriod > ts5Min)
            return 2;
        if (expirationPeriod <= ts1Hour && expirationPeriod > ts30Min)
            return 3;
        if (expirationPeriod <= ts8Hours && expirationPeriod > ts1Hour)
            return 4;

        return 5;
    }


    [BindProperty]
    public List<Guid> ImageIds { get; set; } = [];

    [BindProperty]
    public int SelectedExpirationPeriod { get; set; }

    [BindProperty]
    public string TextContent { get; set; } = string.Empty;

    public List<SelectListItem> ExpirationPeriodOptions
        =>
        [
            new SelectListItem(_localizer["5 minutes"], 1.ToString()),
            new SelectListItem(_localizer["30 minutes"], 2.ToString()),
            new SelectListItem(_localizer["1 hour"], 3.ToString()),
            new SelectListItem(_localizer["8 hours"], 4.ToString()),
            new SelectListItem(_localizer["1 day"], 5.ToString())
        ];


    public AnalyticsModel AnalyticsModel { get; set; } = default!;
    public OpenGraphModel OpenGraphModel { get; set; } = default!;


    private readonly AnalyticsOptions _analyticsOptions;
    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
    private readonly IEntryService _entryService;
    private readonly IStringLocalizer<IndexModel> _localizer;
    private readonly IStringLocalizer<ServerResources> _serverLocalizer;
}