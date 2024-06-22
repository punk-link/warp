using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Text.Json;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/entry")]
public class EntryController : BaseController
{
    public EntryController(IStringLocalizer<ServerResources> localizer, ICookieService cookieService, ICreatorService creatorService,
        IEntryService entryService) : base(localizer)
    {
        _cookieService = cookieService;
        _creatorService = creatorService;
        _entryService = entryService;
    }


    [HttpDelete]
    public async Task<IActionResult> DeleteEntry([FromBody] JsonElement id, CancellationToken cancellationToken = default)
    {
        // TODO: Refactor this line to avoid using TryGetProperty method
        id.TryGetProperty("id", out var value);
        var decodedId = IdCoder.Decode(value.ToString());
        if (decodedId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var creatorId = _cookieService.GetCreatorId(HttpContext);
        if (creatorId is null)
            return NoContent();

        var isEntryBelongsToCreatorResult = await _creatorService.IsEntryBelongsToCreator(creatorId.Value, decodedId, cancellationToken);
        if (isEntryBelongsToCreatorResult.IsFailure)
            return NoContent();

        var result = await _entryService.Remove(decodedId, cancellationToken);
        if (result.IsFailure)
            return Forbid();

        return NoContent();
    }


    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
    private readonly IEntryService _entryService;
}
