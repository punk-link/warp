using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Pages;

public class PreviewModel : BasePageModel
{
    public PreviewModel(ICookieService cookieService,
        ICreatorService creatorService,
        ILoggerFactory loggerFactory)
        : base(cookieService, creatorService, loggerFactory)
    {
    }


    public IActionResult OnGet(string id, CancellationToken cancellationToken)
    {
        Id = id;

        return Page();


        //IActionResult Redirect(Result<(Creator, EntryInfo), DomainError> result)
        //{
        //    if (result.IsFailure)
        //        return RedirectToError(result.Error);

        //    var (creator, entryInfo) = result.Value;
        //    if (entryInfo.CreatorId != creator.Id)
        //        return RedirectToPage("./Entry", new { id = IdCoder.Encode(entryInfo.Id) });

        //    return Page();
        //}
    }


    //public Task<IActionResult> OnPostCopy(string id, CancellationToken cancellationToken)
    //{
    //    return DecodeId(id)
    //        .Bind(decodedId => GetCreator(decodedId, cancellationToken))
    //        .Bind(CopyEntryInfo)
    //        .Finally(result => result.IsSuccess 
    //            ? RedirectToPage("./Index", new { id = IdCoder.Encode(result.Value.Id) }) 
    //            : RedirectToError(result.Error));


    //    Task<Result<EntryInfo, DomainError>> CopyEntryInfo((Creator Creator, Guid DecodedId) tuple) 
    //        => _entryInfoService.Copy(tuple.Creator, tuple.DecodedId, cancellationToken);
    //}


    //public Task<IActionResult> OnPostEdit(string id, CancellationToken cancellationToken)
    //{
    //    return DecodeId(id)
    //        .Bind(decodedId => GetCreator(decodedId, cancellationToken))
    //        .Finally(result => result.IsSuccess 
    //            ? RedirectToPage("./Index", new { id = id }) 
    //            : RedirectToError(result.Error));
    //}
    

    public string Id { get; set; } = default!;
}