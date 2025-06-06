using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.OpenGraph;

namespace Warp.WebApp.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class IndexModel : BasePageModel
{
    public IndexModel(IOptionsSnapshot<AnalyticsOptions> analyticsOptions, 
        ICookieService cookieService, 
        ICreatorService creatorService, 
        IEntryInfoService entryInfoService,
        IStringLocalizer<IndexModel> localizer,
        ILoggerFactory loggerFactory, 
        IOpenGraphService openGraphService)
        : base(cookieService, creatorService, loggerFactory)
    {
        _analyticsOptions = analyticsOptions.Value;
        _cookieService = cookieService;
        _creatorService = creatorService;
        _entryInfoService = entryInfoService;
        _localizer = localizer;
        _openGraphService = openGraphService;
    }


    public Task<IActionResult> OnGet(string? id, CancellationToken cancellationToken)
    {
        return InitializeCreator()
            .Tap(AddAnalyticsModel)
            .Finally(async creatorResult =>
            {
                if (string.IsNullOrEmpty(id))
                    return BuildNewModel();

                var creator = creatorResult.Value;
                return await BuildExistingModel(creator);
            });


        Task<Result<Creator>> InitializeCreator()
        {
            return TryGetCreatorId()
                .Bind(GetOrAddCreator)
                .Tap(SetCreatorCookie);

            
            Result<Guid?> TryGetCreatorId() 
                => _cookieService.GetCreatorId(HttpContext);


            async Task<Result<Creator>> GetOrAddCreator(Guid? creatorId)
            {
                var (isSuccess, _, creator, _) = await _creatorService.Get(creatorId, cancellationToken);
                return isSuccess
                    ? creator
                    : await _creatorService.Add(cancellationToken);
            }


            Task SetCreatorCookie(Creator creator)
                => _cookieService.Set(HttpContext, creator);
        }


        void AddAnalyticsModel()
            => AnalyticsModel = new AnalyticsModel(_analyticsOptions);


        IActionResult BuildNewModel()
        {
            var openGraphDescription = _openGraphService.GetDefaultDescription();
            OpenGraphModel = new OpenGraphModel(openGraphDescription);

            Id = IdCoder.Encode(Guid.CreateVersion7());
            ImageContainers.Add(EditableImageContainerModel.Empty);

            return Page();
        }


        Task<IActionResult> BuildExistingModel(Creator creator)
        {
            return DecodeId(id)
                .Bind(GetEntryInfo)
                .Bind(BuildModel)
                .Tap(AddOpenGraphModel)
                .Finally(result => result.IsSuccess
                    ? Page()
                    : RedirectToError(result.Error));


            Task<Result<EntryInfo, DomainError>> GetEntryInfo(Guid entryId)
                => _entryInfoService.Get(creator, entryId, cancellationToken);


            Result<EntryInfo, DomainError> BuildModel(EntryInfo entryInfo)
            {
                Id = id;
                EditMode = entryInfo.EditMode;
                TextContent = TextFormatter.GetCleanString(entryInfo.Entry.Content);
                SelectedExpirationPeriod = GetExpirationPeriodId(entryInfo.ExpiresAt - entryInfo.CreatedAt);

                foreach (var imageInfo in entryInfo.ImageInfos)
                    ImageContainers.Add(new EditableImageContainerModel(imageInfo));

                ImageContainers.Add(EditableImageContainerModel.Empty);

                return entryInfo;
            }

            
            void AddOpenGraphModel(EntryInfo entryInfo)
                => OpenGraphModel = new OpenGraphModel(entryInfo.OpenGraphDescription);
        }
    }


    public Task<IActionResult> OnPost(CancellationToken cancellationToken)
    {
        return GetCreator(cancellationToken)
            .Bind(BuildEntryRequest)
            .Bind(ProcessEntryRequest)
            .Finally(result =>
            {
                if (result.IsFailure)
                    return RedirectToError(result.Error);

                var entryInfo = result.Value;
                return RedirectToPage("./Preview", new { id = IdCoder.Encode(entryInfo.Id) });
            });


        Result<(Creator, EntryRequest), DomainError> BuildEntryRequest(Creator creator)
        {
            var expiresIn = GetExpirationPeriod(SelectedExpirationPeriod);
            var decodedImageIds = ImageIds.Select(IdCoder.Decode).ToList();
        
            var request = new EntryRequest 
            {
                Id = IdCoder.Decode(Id), 
                EditMode = EditMode, 
                ExpiresIn = expiresIn, 
                ImageIds = decodedImageIds, 
                TextContent = TextContent
            };

            return (creator, request);
        }


        async Task<Result<EntryInfo, DomainError>> ProcessEntryRequest((Creator Creator, EntryRequest EntryRequest) tuple)
        {
            var entryExists = await CheckIfEntryExists(tuple.Creator, tuple.EntryRequest.Id, cancellationToken);
            return entryExists
                ? await _entryInfoService.Update(tuple.Creator, tuple.EntryRequest, cancellationToken)
                : await _entryInfoService.Add(tuple.Creator, tuple.EntryRequest, cancellationToken);


            async Task<bool> CheckIfEntryExists(Creator creator, Guid entryId, CancellationToken cancellationToken)
            {
                var result = await _entryInfoService.Get(creator, entryId, cancellationToken);
                return result.IsSuccess;
            }
        }
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
    public string Id { get; set; }

    [BindProperty]
    public EditMode EditMode { get; set; } = EditMode.Unset;

    [BindProperty]
    public List<string> ImageIds { get; set; } = [];

    [BindProperty]
    public int SelectedExpirationPeriod { get; set; }

    [BindProperty]
    public string TextContent { get; set; } = string.Empty;

    public AnalyticsModel AnalyticsModel { get; set; } = default!;
    public List<EditableImageContainerModel> ImageContainers { get; set; } = [];
    public OpenGraphModel OpenGraphModel { get; set; } = default!;

    public List<SelectListItem> ExpirationPeriodOptions
        =>
        [
            new SelectListItem(_localizer["5 minutes"], 1.ToString()),
            new SelectListItem(_localizer["30 minutes"], 2.ToString()),
            new SelectListItem(_localizer["1 hour"], 3.ToString()),
            new SelectListItem(_localizer["8 hours"], 4.ToString()),
            new SelectListItem(_localizer["1 day"], 5.ToString())
        ];


    private readonly AnalyticsOptions _analyticsOptions;
    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
    private readonly IEntryInfoService _entryInfoService;
    private readonly IStringLocalizer<IndexModel> _localizer;
    private readonly IOpenGraphService _openGraphService;
}
