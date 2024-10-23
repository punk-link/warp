using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Infrastructure;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Helpers;
using Microsoft.Extensions.Localization;

namespace Warp.WebApp.Pages;

public class PreviewModel : BasePageModel
{
    public PreviewModel(ICookieService cookieService,
        ICreatorService creatorService,
        IEntryInfoService entryInfoService,
        ILoggerFactory loggerFactory, 
        IEntryPresentationService entryPresentationService, 
        IStringLocalizer<ServerResources> serverLocalizer, 
        IUrlService urlService)
        : base(cookieService, creatorService, loggerFactory)
    {
        _entryInfoService = entryInfoService;
        _entryPresentationService = entryPresentationService;
        _serverLocalizer = serverLocalizer;
        _urlService = urlService;
    }


    public Task<IActionResult> OnGet(string id, CancellationToken cancellationToken)
    {
        return DecodeId()
            .Bind(GetCreator)
            .Bind(GetEntryInfo)
            .Tap(BuildModel)
            .Finally(Redirect);


        Result<Guid, ProblemDetails> DecodeId()
        {
            var decodedId = IdCoder.Decode(id);
            if (decodedId == Guid.Empty)
                return ProblemDetailsHelper.Create(_serverLocalizer["IdDecodingErrorMessage"]);

            return decodedId;
        }


        Task<Result<(Creator, Guid), ProblemDetails>> GetCreator(Guid decodedId)
            => base.GetCreator(cancellationToken)
                .Map(creator => (creator, decodedId));


        Task<Result<(Creator, EntryInfo), ProblemDetails>> GetEntryInfo((Creator Creator, Guid DecodedId) tuple) 
            => _entryInfoService.Get(tuple.Creator, tuple.DecodedId, cancellationToken)
                .Map(entryInfo => (tuple.Creator, entryInfo));


        void BuildModel((Creator, EntryInfo EntryInfo) tuple)
        {
            Id = id;
            ExpiresIn = new DateTimeOffset(tuple.EntryInfo.Entry.ExpiresAt).ToUnixTimeMilliseconds();
            TextContent = tuple.EntryInfo.Entry.Content;
            
            foreach (var imageId in tuple.EntryInfo.Entry.ImageIds)
                ImageContainers.Add(new ReadOnlyImageContainerModel(_urlService.GetImageUrl(id, imageId)));
        }


        IActionResult Redirect(Result<(Creator, EntryInfo), ProblemDetails> result)
        {
            if (result.IsFailure)
                return RedirectToError(result.Error);

            var (creator, entryInfo) = result.Value;
            if (entryInfo.CreatorId != creator.Id)
                return RedirectToPage("./Entry", new { id = IdCoder.Encode(entryInfo.Entry.Id) });

            return Page();
        }
    }


    public async Task<IActionResult> OnPostCopy(string id, CancellationToken cancellationToken)
    {
        return await _entryPresentationService.Copy(id, HttpContext, cancellationToken)
            .Finally(result => result.IsSuccess 
                ? RedirectToPage("./Index", new { id = IdCoder.Encode(result.Value.Id) }) 
                : RedirectToError(result.Error));
    }


    public long ExpiresIn { get; set; }
    public string Id { get; set; } = default!;
    public List<ReadOnlyImageContainerModel> ImageContainers { get; set; } = [];
    public string TextContent { get; set; } = string.Empty;


    private readonly IEntryInfoService _entryInfoService;
    private readonly IEntryPresentationService _entryPresentationService;
    private readonly IStringLocalizer<ServerResources> _serverLocalizer;
    private readonly IUrlService _urlService;
}