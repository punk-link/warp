using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/reports")]
public sealed class ReportController : BaseController
{
    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }
    
    
    [HttpPost("{id}")]
    public async Task<IActionResult> Post([FromRoute] string id)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        await _reportService.MarkAsReported(decodedId);
        return NoContent();
    }

    
    private readonly IReportService _reportService;
}