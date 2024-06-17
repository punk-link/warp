using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/reports")]
public sealed class ReportController : BaseController
{
    public ReportController(IStringLocalizer<ServerResources> localizer, IReportService reportService) : base(localizer)
    {
        _reportService = reportService;
    }


    [HttpPost("{id}")]
    public async Task<IActionResult> Post([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        await _reportService.MarkAsReported(decodedId, cancellationToken);
        return NoContent();
    }


    private readonly IReportService _reportService;
}