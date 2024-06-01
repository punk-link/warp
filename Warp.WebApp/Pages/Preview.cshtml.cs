using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Pages.Shared.Components;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Pages
{
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

            var claim = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name && Guid.TryParse(x.Value, out _));
            if (claim != null)
            {
                var userGuid = Guid.Parse(claim.Value);
                var (_, isFailure, entry, problemDetails) = await _entryService.Get(userGuid, decodedId, cancellationToken);
                if (isFailure)
                    return RedirectToError(problemDetails);

                Result.Success()
                    .Tap(() => BuildModel(id, entry))
                    .Tap(AddOpenGraphModel);

                return Page();
            }

            return RedirectToError(ProblemDetailsHelper.Create("Can`t open preview page cause of no permission."));

            void BuildModel(string entryId, EntryInfo entryInfo)
            {
                Id = entryId;
                ExpiresIn = new DateTimeOffset(entryInfo.Entry.ExpiresAt).ToUnixTimeMilliseconds();
                TextContent = entryInfo.Entry.Content;
                ImageUrls = BuildImageUrls(decodedId, entryInfo.ImageIds);
                EntryInfo = entryInfo;
            }


            void AddOpenGraphModel()
            {
                OpenGraphModel = OpenGraphService.GetModel(TextContent, ImageUrls);
            }
        }

        public IActionResult OnPostEdit(string id)
        {
            return RedirectToPage("./Index", new { id });
        }

        public async Task<IActionResult> OnPostDelete(string id, CancellationToken cancellationToken)
        {
            var decodedId = IdCoder.Decode(id);
            if (decodedId == Guid.Empty)
                return RedirectToError(ProblemDetailsHelper.Create("Can't decode a provided ID."));

            var (_, isFailure, _, problemDetails) = await _entryService.Remove(decodedId, cancellationToken);

            if (isFailure)
                return RedirectToError(problemDetails);

            //var claims = HttpContext.User.Claims.ToList();
            //var claim = claims.FirstOrDefault(x => x.Type == ClaimTypes.UserData && x.Value == decodedId.ToString());

            //if (claim != null)
                await Response.HttpContext.SignOutAsync();

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostCopy(EntryInfo entryInfo, CancellationToken cancellationToken)
        {
            var claim = this.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name && Guid.TryParse(x.Value, out _));
            if (claim != null)
            {
                var userGuid = Guid.Parse(claim.Value);
                var (_, isFailure, id, problemDetails) = await _entryService.Add(userGuid, entryInfo.Entry.Content, entryInfo.Entry.ExpiresAt - entryInfo.Entry.CreatedAt, entryInfo.ImageIds, cancellationToken);
                if(isFailure)
                    return RedirectToError(problemDetails);

                return RedirectToPage("./Index", new { id = IdCoder.Encode(id) });
            }

            return RedirectToError(ProblemDetailsHelper.Create("Can`t copy entry cause of no permission."));
        }


        private static List<string> BuildImageUrls(Guid id, List<Guid> imageIds)
            => imageIds.Select(imageId => $"/api/images/entry-id/{id}/image-id/{imageId}")
                .ToList();


        public OpenGraphModel OpenGraphModel { get; set; } = default!;


        public long ExpiresIn { get; set; }
        public string Id { get; set; } = default!;
        public List<string> ImageUrls { get; set; } = [];
        public string TextContent { get; set; } = string.Empty;
        public EntryInfo EntryInfo { get; set; } = default;


        private readonly IEntryService _entryService;

    }
}
