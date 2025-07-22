using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Attributes;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Converters;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.OpenGraph;

namespace Warp.WebApp.Controllers;

/// <summary>
/// Controller for managing entry resources, including creation, retrieval, update, copy, deletion, and reporting of entries.
/// </summary>
[ApiController]
[Route("/api/entries")]
public class EntryController : BaseController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntryController"/> class.
    /// </summary>
    /// <param name="cookieService">The cookie service.</param>
    /// <param name="creatorService">The creator service.</param>
    /// <param name="entryInfoService">The entry info service.</param>
    /// <param name="openGraphService">The open graph service.</param>
    /// <param name="reportService">The report service.</param>
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


    /// <summary>
    /// Adds or updates an entry.
    /// </summary>
    /// <param name="id">The encoded entry identifier.</param>
    /// <param name="request">The entry API request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The entry response or error.</returns>
    [HttpPost("{id}")]
    [ProducesResponseType(typeof(EntryApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DomainError), StatusCodes.Status400BadRequest)]
    [RequireCreatorCookie]
    [ValidateId]
    public async Task<IActionResult> AddOrUpdate([FromRoute] string id, [FromBody] EntryApiRequest request, CancellationToken cancellationToken = default)
    {
        var creator = await GetCreator(cancellationToken);
        
        var decodedId = IdCoder.Decode(id);
        var req = request.ToEntryRequest(decodedId);

        // TODO: replace this check with a more performant version
        var entryInfo = await _entryInfoService.Get(creator, decodedId, cancellationToken);
        Result<EntryInfo, DomainError> result;
        if (entryInfo.IsFailure)
            result = await _entryInfoService.Add(creator, req, cancellationToken);
        else
            result = await _entryInfoService.Update(creator, req, cancellationToken);

        return OkOrBadRequest(result.ToEntryApiResponse());
    }


    /// <summary>
    /// Copies an entry by its identifier.
    /// </summary>
    /// <param name="id">The encoded entry identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The copied entry response or error.</returns>
    [HttpPost("{id}/copy")]
    [ProducesResponseType(typeof(EntryApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DomainError), StatusCodes.Status400BadRequest)]
    [RequireCreatorCookie]
    [ValidateId]
    public async Task<IActionResult> Copy([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        var creator = await GetCreator(cancellationToken);
        var result = await _entryInfoService.Copy(creator, decodedId, cancellationToken)
            .ToEntryApiResponse();

        return OkOrBadRequest(result);
    }


    /// <summary>
    /// Creates a new entry with a unique identifier and default description.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(EntryApiResponse), StatusCodes.Status200OK)]
    [RequireCreatorCookie]
    public IActionResult Create()
    {
        var encodedId = IdCoder.Encode(Guid.CreateVersion7());
        var defaultDescription = _openGraphService.GetDefaultDescription();

        return Ok(EntryApiResponse.Empty(encodedId, defaultDescription));
    }


    /// <summary>
    /// Deletes an entry by its identifier.
    /// </summary>
    /// <param name="id">The encoded entry identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [RequireCreatorCookie]
    [ValidateId]
    public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        var creator = await GetCreator(cancellationToken);
        _ = await _entryInfoService.Remove(creator, decodedId, cancellationToken);

        return NoContent();
    }


    /// <summary>
    /// Gets an entry by its identifier or returns a new entry response if the identifier is invalid.
    /// </summary>
    /// <param name="id">The encoded entry identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The entry response or error.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EntryApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DomainError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        if (decodedId == Guid.Empty)
            return Create();

        var creator = await GetCreator(cancellationToken);
        var result = await _entryInfoService.Get(creator, decodedId, cancellationToken)
            .ToEntryApiResponse();

        return OkOrBadRequest(result);
    }


    /// <summary>
    /// Checks if an entry is editable by its identifier.
    /// </summary>
    /// <param name="id"> The encoded entry identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A boolean indicating if the entry is editable.</returns>
    [HttpGet("{id}/is-editable")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ValidateId]
    public async Task<IActionResult> IsEditable([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        var creator = await GetCreator(cancellationToken);
        
        var isEditableResult = await _entryInfoService.IsEditable(creator, decodedId, cancellationToken);

        return Ok(isEditableResult.IsSuccess);
    }


    /// <summary>
    /// Reports an entry by its identifier for inappropriate content.
    /// </summary>
    /// <param name="id">The encoded entry identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpPost("{id}/report")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [RequireCreatorCookie]
    [ValidateId]
    public async Task<IActionResult> Report([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var decodedId = IdCoder.Decode(id);
        await _reportService.MarkAsReported(decodedId, cancellationToken);

        return NoContent();
    }


    private readonly IEntryInfoService _entryInfoService;
    private readonly IOpenGraphService _openGraphService;
    private readonly IReportService _reportService;
}
