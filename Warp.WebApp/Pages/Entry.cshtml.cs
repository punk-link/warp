using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Infrastructure;

namespace Warp.WebApp.Pages;

public class EntryModel : BasePageModel
{
    public EntryModel(ICookieService cookieService, 
        ICreatorService creatorService,
        IEntryInfoService entryPresentationService,
        ILoggerFactory loggerFactory, 
        IStringLocalizer<ServerResources> serverLocalizer, 
        IUrlService urlService)
        : base(cookieService, creatorService, loggerFactory, serverLocalizer)
    {
        _entryPresentationService = entryPresentationService;
        _urlService = urlService;
    }


    public Task<IActionResult> OnGet(string id, CancellationToken cancellationToken)
    {
        return DecodeId(id)
            .Bind(GetCreator)
            .Bind(GetEntryInfo)
            .Bind(BuildModel)
            .Finally(result => result.IsSuccess
                ? Page()
                : RedirectToError(result.Error));


        Task<Result<(Creator, Guid), ProblemDetails>> GetCreator(Guid decodedId) 
            => base.GetCreator(cancellationToken)
                .OnFailureCompensate(() => Creator.Empty)
                .Map(creator => (creator, decodedId));


        Task<Result<EntryInfo, ProblemDetails>> GetEntryInfo((Creator Creator, Guid DecodedId) tuple) 
            => _entryPresentationService.Get(tuple.Creator, tuple.DecodedId, cancellationToken);


        Result<EntryInfo, ProblemDetails> BuildModel(EntryInfo entryInfo)
        {
            Id = id;
            ExpiresIn = new DateTimeOffset(entryInfo.Entry.ExpiresAt).ToUnixTimeMilliseconds();
            OpenGraphModel = new OpenGraphModel(entryInfo.OpenGraphDescription);
            TextContent = entryInfo.Entry.Content;
            ViewCount = entryInfo.ViewCount;

            foreach (var imageId in entryInfo.Entry.ImageIds)
                ImageContainers.Add(new ReadOnlyImageContainerModel(_urlService.GetImageUrl(id, imageId)));

            return entryInfo;
        }
    }


    public OpenGraphModel OpenGraphModel { get; set; } = default!;


    public long ExpiresIn { get; set; }
    public string Id { get; set; } = default!;
    public List<ReadOnlyImageContainerModel> ImageContainers { get; set; } = [];
    public string TextContent { get; set; } = string.Empty;
    public long ViewCount { get; set; } = 1;


    private readonly IEntryInfoService _entryPresentationService;
    private readonly IUrlService _urlService;
}