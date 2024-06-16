using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Warp.WebApp.Helpers;
using Warp.WebApp.Services.User;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Localization;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/entry")]
public class EntryController : BaseController
{
    public EntryController(IStringLocalizer<ServerResources> localizer, IEntryService entryService) : base(localizer)
    {
        _entryService = entryService;
    }


    [HttpDelete]
    public async Task<IActionResult> DeleteEntry([FromBody] JsonElement id, CancellationToken cancellationToken = default)
    {
        id.TryGetProperty("id", out var value);
        var decodedEntryId = IdCoder.Decode(value.ToString());
        if (decodedEntryId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var claim = CookieService.GetClaim(HttpContext);
        if (claim != null)
        {
            var userId = Guid.Parse(claim.Value);
            var result = await _entryService.Remove(userId, decodedEntryId, cancellationToken);
            if (result.IsFailure)
                return Forbid();
        }

        return Ok();
    }


    private readonly IEntryService _entryService;
}
