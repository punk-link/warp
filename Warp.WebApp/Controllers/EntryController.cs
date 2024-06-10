using Microsoft.AspNetCore.Mvc;
using System.Threading;
using Warp.WebApp.Helpers;
using Warp.WebApp.Services.User;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/entry")]
public class EntryController : BaseController
{
    public EntryController(IEntryService entryService)
    {
        _entryService = entryService;
    }


    [HttpDelete]
    public IActionResult DeleteEntry([FromForm] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return RedirectToPage("./Error");

        var claim = CookieService.GetClaim(HttpContext);
        if (claim != null)
        {
            _entryService.Remove(decodedId, cancellationToken);
        }

        return RedirectToPage("./Index");
    }


    private readonly IEntryService _entryService;
}
