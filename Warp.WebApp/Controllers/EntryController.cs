using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Attributes;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.OpenGraph;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/entries")]
[RequireCreatorCookie]
public class EntryController : BaseController
{
    public EntryController(ICookieService cookieService, 
        ICreatorService creatorService,
        IEntryInfoService entryInfoService,
        IOpenGraphService openGraphService,
        IReportService reportService) : base(cookieService, creatorService)
    {
        _entryInfoService = entryInfoService;
        _openGraphService = openGraphService;
        _reportService = reportService;
    }


    [HttpPost("{id}/copy")]
    [ValidateDecodableId]
    public async Task<IActionResult> Copy([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        var creator = await GetCreator(cancellationToken);
        var result = await _entryInfoService.Copy(creator, decodedId, cancellationToken);
        if (result.IsFailure)
            return BadRequest(result.Error);

        var response = new EntryApiResponse(id, result.Value);
        return Ok(response);
    }


    [HttpGet]
    public IActionResult Create() 
        => ReturnNew();


    [HttpDelete("{id}")]
    [ValidateDecodableId]
    public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        var creator = await GetCreator(cancellationToken);
        _ = await _entryInfoService.Remove(creator, decodedId, cancellationToken);

        return NoContent();
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] string? id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return ReturnNew();

        return await ReturnExisting();


        async Task<IActionResult> ReturnExisting()
        {
            var creator = await GetCreator(cancellationToken);
            var (_, isFailure, entry, error) = await _entryInfoService.Get(creator, decodedId, cancellationToken);
            if (isFailure)
                return BadRequest(error);

            var response = new EntryApiResponse(id!, entry);
            return Ok(response);
        }
    }


    [HttpPost("{id}")]
    [ValidateDecodableId]
    public async Task<IActionResult> Post([FromRoute] string id, [FromBody] EntryApiRequest request, CancellationToken cancellationToken = default)
    {
        var req = new EntryRequest()
        {
            Id = decodedId,
            EditMode = request.EditMode,
            ExpiresIn = GetExpirationPeriod(request.ExpirationPeriod),
            ImageIds = request.ImageIds,
            TextContent = request.TextContent
        };
        
        var creator = await GetCreator(cancellationToken);
        var decodedId = IdCoder.Decode(id);
        var isExistResult = await _entryInfoService.Get(creator, decodedId, cancellationToken);

        Result<EntryInfo, DomainError> postResult;
        if (isExistResult.IsSuccess)
            postResult = await _entryInfoService.Update(creator, req, cancellationToken);
        else
            postResult = await _entryInfoService.Add(creator, req, cancellationToken);

        return OkOrBadRequest(postResult);


        static TimeSpan GetExpirationPeriod(ExpirationPeriod expirationPeriod)
            => expirationPeriod switch
            {
                ExpirationPeriod.FiveMinutes => new TimeSpan(0, 5, 0),
                ExpirationPeriod.ThirtyMinutes => new TimeSpan(0, 30, 0),
                ExpirationPeriod.OneHour => new TimeSpan(1, 0, 0),
                ExpirationPeriod.EightHours => new TimeSpan(8, 0, 0),
                ExpirationPeriod.OneDay => new TimeSpan(24, 0, 0),
                _ => new TimeSpan(0, 5, 0)
            };
    }


    [HttpPost("{id}/report")]
    [ValidateDecodableId]
    public async Task<IActionResult> Report([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        await _reportService.MarkAsReported(decodedId, cancellationToken);
        return NoContent();
    }


    private OkObjectResult ReturnNew()
    {
        var id = Guid.CreateVersion7();
        var encodedId = IdCoder.Encode(id);

        var openGraphDescription = _openGraphService.GetDefaultDescription();

        var response = new EntryApiResponse(encodedId, openGraphDescription);
        return Ok(response);
    }


    private readonly IEntryInfoService _entryInfoService;
    private readonly IOpenGraphService _openGraphService;
    private readonly IReportService _reportService;
}
