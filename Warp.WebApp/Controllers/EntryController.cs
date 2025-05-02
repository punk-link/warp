using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/entries")]
public class EntryController : BaseController
{
    public EntryController(ICookieService cookieService, 
        ICreatorService creatorService,
        IEntryInfoService entryInfoService, 
        IReportService reportService) : base(cookieService, creatorService)
    {
        _entryInfoService = entryInfoService;
        _reportService = reportService;
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntry([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return IdDecodingBadRequest();

        var (_, isFailure, creator, error) = await GetCreator(cancellationToken);
        if (isFailure)
            return Unauthorized(error);

        _ = await _entryInfoService.Remove(creator, decodedId, cancellationToken);
        return Ok();
    }


    [HttpPost("{id}/report")]
    public async Task<IActionResult> ReportEntry([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return IdDecodingBadRequest();

        await _reportService.MarkAsReported(decodedId, cancellationToken);
        return NoContent();
    }


    private readonly IEntryInfoService _entryInfoService;
    private readonly IReportService _reportService;
}
