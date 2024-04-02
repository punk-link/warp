using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Pages;

public class EntryModel : BasePageModel
{
    public EntryModel(ILoggerFactory loggerFactory, IEntryService entryService) 
        : base(loggerFactory)
    {
        _entryService = entryService;
    }
    
    
    public async Task<IActionResult> OnGet(string id, CancellationToken cancellationToken)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return RedirectToError(ProblemDetailsHelper.Create("Can't decode a provided ID."));

        var (_, isFailure, entry, problemDetails) = await _entryService.Get(decodedId, cancellationToken);
        if (isFailure)
            return RedirectToError(problemDetails);

        Result.Success()
            .Tap(() => BuildModel(id, entry))
            .Tap(AddButtonModels)
            .Tap(AddModalWindowModel)
            .Tap(AddOpenGraphModel);

        return Page();


        void BuildModel(string entryId, EntryInfo entryInfo)
        {
            Id = entryId;
            ExpiresIn = GetExpirationMessage(entryInfo.Entry.ExpiresAt);
            TextContent = TextFormatter.Format(entryInfo.Entry.Content);

            ViewCount = entryInfo.ViewCount;
            ImageUrls = BuildImageUrls(decodedId, entryInfo.ImageIds);
        }


        void AddButtonModels()
        {
            CopyButtonModel = new SecondaryButtonModel
            {
                Id = "copy-url-button",
                IconName = "icofont-copy",
                MainCaption = "copy link",
                SecondaryCaption = "copied",
                TabIndex = 1
            };

            CopySilentButtonModel = new SecondaryButtonModel
            {
                Id = "copy-url-silent-button",
                IconName = "icofont-copy",
                MainCaption = "copy link",
                TabIndex = 2
            };

            ReportButtonModel = new SecondaryButtonModel
            {
                Id = "report-button",
                MainCaption = "report",
                TabIndex = 3
            };
        }


        void AddModalWindowModel()
        {
            ModalWindowModel = new ModalWindowModel
            {
                Action = "report",
                Header = "report entry",
                Prompt = "You are about to report this content. This action restricts an access to the content for all viewers. Are you sure?",
                TabIndex = 4,
                CancelTabIndex = 5
            };
        }


        void AddOpenGraphModel()
        {
            OpenGraphModel = OpenGraphService.GetModel(TextContent, ImageUrls);
        }
    }


    private static List<string> BuildImageUrls(Guid id, List<Guid> imageIds)
        => imageIds.Select(imageId => $"/api/images/entry-id/{id}/image-id/{imageId}")
            .ToList();


    private static string GetExpirationMessage(DateTime expiresAt)
    {
        var timeSpan = GetExpirationTimeSpan(expiresAt);
        if (1 < timeSpan.Days)
            return $"The entry expires in {timeSpan.Days} days";

        if (1 < timeSpan.Hours)
            return $"The entry expires in {timeSpan.Hours} hours";

        if (1 < timeSpan.Minutes)
            return $"The entry expires in {timeSpan.Minutes} minutes";

        if (1 < timeSpan.Seconds)
            return $"The entry expires in {timeSpan.Seconds} seconds";
        
        return "The entry expires now";
    }


    private static TimeSpan GetExpirationTimeSpan(DateTime expiresAt)
        => expiresAt - DateTime.UtcNow;

    
    public SecondaryButtonModel CopyButtonModel { get; set; } = default!;
    public SecondaryButtonModel CopySilentButtonModel { get; set; } = default!;
    public SecondaryButtonModel ReportButtonModel { get; set; } = default!;
    public ModalWindowModel ModalWindowModel { get; set; } = default!;
    public OpenGraphModel OpenGraphModel { get; set; } = default!;


    public string ExpiresIn { get; set; } = string.Empty;
    public string Id { get; set; } = default!;
    public List<string> ImageUrls { get; set; } = [];
    public string TextContent { get; set; } = string.Empty;
    public long ViewCount { get; set; } = 1;
    
    
    private readonly IEntryService _entryService;
}