using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Infrastructure;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Models.Creators;
using Microsoft.Extensions.Localization;

namespace Warp.WebApp.Pages;

public class PreviewModel : BasePageModel
{
    public PreviewModel(ICookieService cookieService,
        ICreatorService creatorService,
        IEntryInfoService entryInfoService,
        ILoggerFactory loggerFactory, 
        IStringLocalizer<ServerResources> serverLocalizer, 
        IUrlService urlService)
        : base(cookieService, creatorService, loggerFactory, serverLocalizer)
    {
        _entryInfoService = entryInfoService;
        _urlService = urlService;
    }


    public Task<IActionResult> OnGet(string id, CancellationToken cancellationToken)
    {
        return DecodeId(id)
            .Bind(decodedId => GetCreator(decodedId, cancellationToken))
            .Bind(GetEntryInfo)
            .Tap(BuildModel)
            .Finally(Redirect);


        Task<Result<(Creator, EntryInfo), ProblemDetails>> GetEntryInfo((Creator Creator, Guid DecodedId) tuple) 
            => _entryInfoService.Get(tuple.Creator, tuple.DecodedId, cancellationToken)
                .Map(entryInfo => (tuple.Creator, entryInfo));


        void BuildModel((Creator, EntryInfo EntryInfo) tuple)
        {
            Id = id;
            ExpiresIn = new DateTimeOffset(tuple.EntryInfo.ExpiresAt).ToUnixTimeMilliseconds();
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
                return RedirectToPage("./Entry", new { id = IdCoder.Encode(entryInfo.Id) });

            return Page();
        }
    }


    public Task<IActionResult> OnPostCopy(string id, CancellationToken cancellationToken)
    {
        return DecodeId(id)
            .Bind(decodedId => GetCreator(decodedId, cancellationToken))
            .Bind(CopyEntryInfo)
            .Finally(result => result.IsSuccess 
                ? RedirectToPage("./Index", new { id = IdCoder.Encode(result.Value.Id) }) 
                : RedirectToError(result.Error));


        Task<Result<EntryInfo, ProblemDetails>> CopyEntryInfo((Creator Creator, Guid DecodedId) tuple) 
            => _entryInfoService.Copy(tuple.Creator, tuple.DecodedId, cancellationToken);
    }


    private Task<Result<(Creator, Guid), ProblemDetails>> GetCreator(Guid decodedId, CancellationToken cancellationToken) 
        => GetCreator(cancellationToken)
            .Map(creator => (creator, decodedId));


    public long ExpiresIn { get; set; }
    public string Id { get; set; } = default!;
    public List<ReadOnlyImageContainerModel> ImageContainers { get; set; } = [];
    public string TextContent { get; set; } = string.Empty;


    private readonly IEntryInfoService _entryInfoService;
    private readonly IUrlService _urlService;
}