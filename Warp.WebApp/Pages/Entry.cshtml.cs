using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Pages;

public class EntryModel : BasePageModel
{
    public EntryModel(ICookieService cookieService, 
        ICreatorService creatorService,
        IEntryInfoService entryPresentationService,
        ILoggerFactory loggerFactory)
        : base(cookieService, creatorService, loggerFactory)
    {
        _entryPresentationService = entryPresentationService;
    }


    public Task<IActionResult> OnGet(string id, CancellationToken cancellationToken)
    {
        return DecodeId(id)
            .Bind(GetCreator)
            .Bind(GetEntryInfo)
            .Bind(BuildOpenGraphModel)
            .Finally(result => result.IsSuccess
                ? Page()
                : RedirectToError(result.Error));


        Task<Result<(Creator, Guid), DomainError>> GetCreator(Guid decodedId) 
            => base.GetCreator(cancellationToken)
                .OnFailureCompensate(() => Creator.Empty)
                .Map(creator => (creator, decodedId));


        Task<Result<EntryInfo, DomainError>> GetEntryInfo((Creator Creator, Guid DecodedId) tuple) 
            => _entryPresentationService.Get(tuple.Creator, tuple.DecodedId, cancellationToken);


        Result<EntryInfo, DomainError> BuildOpenGraphModel(EntryInfo entryInfo)
        {
            Id = id;
            OpenGraphModel = new OpenGraphModel(entryInfo.OpenGraphDescription);
            return entryInfo;
        }
    }

    
    public string Id { get; set; } = default!;
    public OpenGraphModel OpenGraphModel { get; set; } = default!;


    private readonly IEntryInfoService _entryPresentationService;
}