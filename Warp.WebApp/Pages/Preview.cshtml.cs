using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Pages
{
    [Authorize]
    public class PreviewModel : BasePageModel
    {
        public PreviewModel(ILoggerFactory loggerFactory, IEntryService previewEntryService) : base(loggerFactory)
        {
            _entryService = previewEntryService;
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
                .Tap(AddOpenGraphModel);

            return Page();


            void BuildModel(string entryId, EntryInfo entryInfo)
            {
                Id = entryId;
                ExpiresIn = new DateTimeOffset(entryInfo.Entry.ExpiresAt).ToUnixTimeMilliseconds();
                TextContent = entryInfo.Entry.Content;
                ImageUrls = BuildImageUrls(decodedId, entryInfo.ImageIds);
            }


            void AddOpenGraphModel()
            {
                OpenGraphModel = OpenGraphService.GetModel(TextContent, ImageUrls);
            }
        }


        private static List<string> BuildImageUrls(Guid id, List<Guid> imageIds)
            => imageIds.Select(imageId => $"/api/images/entry-id/{id}/image-id/{imageId}")
                .ToList();


        public OpenGraphModel OpenGraphModel { get; set; } = default!;


        public long ExpiresIn { get; set; }
        public string Id { get; set; } = default!;
        public List<string> ImageUrls { get; set; } = [];
        public string TextContent { get; set; } = string.Empty;


        private readonly IEntryService _entryService;

    }
}
