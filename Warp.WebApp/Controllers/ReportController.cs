using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Services;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/reports")]

public class ReportController : BaseController
{
    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }
    
    
    [HttpPost("{id:guid}")]
    public IActionResult Post([FromRoute] Guid id)
    {
        _reportService.MarkAsReported(id);
        return NoContent();
    }

    
    private readonly IReportService _reportService;
}