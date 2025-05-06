using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Pages;

public class PreviewModel : BasePageModel
{
    public PreviewModel(ICookieService cookieService,
        ICreatorService creatorService,
        IEntryInfoService entryInfoService,
        ILoggerFactory loggerFactory)
        : base(cookieService, creatorService, loggerFactory)
    {
        _entryInfoService = entryInfoService;
    }


    public Task<IActionResult> OnGet(string id, CancellationToken cancellationToken)
    {
        return DecodeId(id)
            .Bind(decodedId => GetCreator(decodedId, cancellationToken))
            .Bind(GetEntryInfo)
            .Tap(BuildModel)
            .Finally(Redirect);


        Task<Result<(Creator, EntryInfo), DomainError>> GetEntryInfo((Creator Creator, Guid DecodedId) tuple) 
            => _entryInfoService.Get(tuple.Creator, tuple.DecodedId, cancellationToken)
                .Map(entryInfo => (tuple.Creator, entryInfo));


        void BuildModel((Creator Creator, EntryInfo EntryInfo) tuple)
        {
            Id = id;
            ExpiresIn = new DateTimeOffset(tuple.EntryInfo.ExpiresAt).ToUnixTimeMilliseconds();
            TextContent = tuple.EntryInfo.Entry.Content;
            CanEdit = tuple.EntryInfo.CreatorId == tuple.Creator.Id && tuple.EntryInfo.ViewCount == 0;
            
            foreach (var imageInfo in tuple.EntryInfo.ImageInfos)
                ImageContainers.Add(new ReadOnlyImageContainerModel(imageInfo));
        }


        IActionResult Redirect(Result<(Creator, EntryInfo), DomainError> result)
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


        Task<Result<EntryInfo, DomainError>> CopyEntryInfo((Creator Creator, Guid DecodedId) tuple) 
            => _entryInfoService.Copy(tuple.Creator, tuple.DecodedId, cancellationToken);
    }


    public Task<IActionResult> OnPostEdit(string id, CancellationToken cancellationToken)
    {
        return DecodeId(id)
            .Bind(decodedId => GetCreator(decodedId, cancellationToken))
            .Finally(result => result.IsSuccess 
                ? RedirectToPage("./Index", new { id = id }) 
                : RedirectToError(result.Error));
    }


    private Task<Result<(Creator, Guid), DomainError>> GetCreator(Guid decodedId, CancellationToken cancellationToken) 
        => GetCreator(cancellationToken)
            .Map(creator => (creator, decodedId));
    

    public bool CanEdit { get; set; }
    public long ExpiresIn { get; set; }
    public string Id { get; set; } = default!;
    public List<ReadOnlyImageContainerModel> ImageContainers { get; set; } = [];
    public string TextContent { get; set; } = string.Empty;


    private readonly IEntryInfoService _entryInfoService;
}