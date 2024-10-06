using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/entries")]
public class EntryController : BaseController
{
    public EntryController(IStringLocalizer<ServerResources> localizer, ICookieService cookieService, ICreatorService creatorService,
        IEntryService entryService, IReportService reportService) : base(localizer)
    {
        _cookieService = cookieService;
        _creatorService = creatorService;
        _entryService = entryService;
        _reportService = reportService;
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntry([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var creatorId = _cookieService.GetCreatorId(HttpContext);
        if (!creatorId.HasValue)
            return ReturnForbid();

        var entryBelongsToCreator = await _creatorService.EntryBelongsToCreator(creatorId.Value, decodedId, cancellationToken);
        if (!entryBelongsToCreator)
            return ReturnForbid();

        _ = await _entryService.Remove(decodedId, cancellationToken);
        return Ok();
    }


    [HttpPost("{id}/report")]
    public async Task<IActionResult> ReportEntry([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        await _reportService.MarkAsReported(decodedId, cancellationToken);
        return NoContent();
    }


    private readonly ICookieService _cookieService;
    private readonly ICreatorService _creatorService;
    private readonly IEntryService _entryService;
    private readonly IReportService _reportService;
}
