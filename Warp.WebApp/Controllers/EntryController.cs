using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Warp.WebApp.Attributes;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Converters;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models.Options;

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
    /// <param name="reportService">The report service.</param>
    public EntryController(
        ILoggerFactory loggerFactory, 
        IOptions<ImageUploadOptions> imageOptions, 
        ICookieService cookieService, 
        ICreatorService creatorService,
        IEntryInfoService entryInfoService,
        IReportService reportService,
        IUnauthorizedImageService unauthorizedImageService) : base(cookieService, creatorService)
    {
        _imageOptions = imageOptions.Value;

        _entryInfoService = entryInfoService;
        _loggerFactory = loggerFactory;
        _reportService = reportService;
        _unauthorizedImageService = unauthorizedImageService;
    }


    /// <summary>
    /// Adds or updates an entry with optional image files supplied in the same multipart request.
    /// </summary>
    /// <param name="id">The encoded entry identifier.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Returns the added or updated entry response or error.</returns>
    [HttpPost("{id}")]
    [ProducesResponseType(typeof(EntryApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DomainError), StatusCodes.Status400BadRequest)]
    [RequireCreatorCookie]
    [ValidateId]
    [MultipartFormData]
    //[IdempotentRequest] // TODO: implement idempotency for multipart requests
    [DisableFormValueModelBinding]
    [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> AddOrUpdateWithImages([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        if (Request.ContentType is null || !MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            return BadRequest(DomainErrors.MultipartContentTypeBoundaryError());

        var boundaryResult = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), _imageOptions.RequestBoundaryLengthLimit);
        if (boundaryResult.IsFailure)
            return BadRequest(boundaryResult.Error);

        var multipartReader = new MultipartReader(boundaryResult.Value, HttpContext.Request.Body);
        if (multipartReader is null)
            return BadRequest(DomainErrors.MultipartReaderError());

        var formValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var existingImageIds = new List<string>(_imageOptions.MaxFileCount);
        var uploadedImageIds = new List<string>(_imageOptions.MaxFileCount);
        var fileHelper = new FileHelper(_loggerFactory, _imageOptions.AllowedExtensions, _imageOptions.MaxFileSize);

        // DO NOT refactor this to separate method - relies on memory management specific to file uploads.
        MultipartSection? section;
        do
        {
            section = await multipartReader.ReadNextSectionAsync(cancellationToken);
            if (section is null)
                break;

            if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                continue;

            if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
            {
                var appFileResult = await fileHelper.ProcessStreamedFile(section, contentDisposition);
                if (appFileResult.IsFailure)
                    continue; // skip failed file silently (could collect errors)

                var decodedEntryId = IdCoder.Decode(id);
                var addResult = await _unauthorizedImageService.Add(decodedEntryId, appFileResult.Value, cancellationToken);
                if (addResult.IsSuccess)
                    uploadedImageIds.Add(IdCoder.Encode(addResult.Value.Id));
            }
            else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
            {
                using var formDataReader = new StreamReader(section.Body);
                var value = await formDataReader.ReadToEndAsync(cancellationToken);
                var key = contentDisposition.Name.Value?.Trim('"');
                if (!string.IsNullOrEmpty(key))
                {
                    if (string.Equals(key, "imageIds", StringComparison.OrdinalIgnoreCase))
                        existingImageIds.Add(value);
                    else
                        formValues[key] = value;
                }
            }
        } while (section is not null);

        var apiRequest = CreateEntryApiRequest(formValues, uploadedImageIds, existingImageIds);
        return await AddOrUpdateInternal(id, apiRequest, cancellationToken);


        static EntryApiRequest CreateEntryApiRequest(Dictionary<string, string> formValues, List<string> uploadedImageIds, List<string> existingImageIds)
        {
            var editModeStr = formValues.TryGetValue("editMode", out var editModeValue) ? editModeValue : "0";
            var expirationStr = formValues.TryGetValue("expirationPeriod", out var expirationPeriodValue) ? expirationPeriodValue : "0";
            var textContent = formValues.TryGetValue("textContent", out var textContentValue) ? textContentValue : string.Empty;

            Enum.TryParse(editModeStr, true, out Models.Entries.Enums.EditMode editMode);
            Enum.TryParse(expirationStr, true, out Models.Entries.Enums.ExpirationPeriod expirationPeriod);

            var allImageIds = new List<string>(existingImageIds.Count + uploadedImageIds.Count);
            allImageIds.AddRange(existingImageIds);
            allImageIds.AddRange(uploadedImageIds);

            return new EntryApiRequest
            {
                EditMode = editMode,
                ExpirationPeriod = expirationPeriod,
                ImageIds = allImageIds,
                TextContent = textContent
            };
        }
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
        return Ok(EntryApiResponse.Empty(encodedId));
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

        var creator = await GetCreatorOrDefault(cancellationToken);
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

        return OkOrBadRequest(isEditableResult);
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


    private async Task<IActionResult> AddOrUpdateInternal(string id, EntryApiRequest request, CancellationToken cancellationToken)
    {
        var creator = await GetCreator(cancellationToken);
        var decodedId = IdCoder.Decode(id);
        var entryRequest = request.ToEntryRequest(decodedId);

        var entryInfo = await _entryInfoService.Get(creator, decodedId, cancellationToken);
        var result = entryInfo.IsFailure
            ? await _entryInfoService.Add(creator, entryRequest, cancellationToken)
            : await _entryInfoService.Update(creator, entryRequest, cancellationToken);

        return OkOrBadRequest(result.ToEntryApiResponse());
    }


    private readonly IEntryInfoService _entryInfoService;
    private readonly IReportService _reportService;
    private readonly IUnauthorizedImageService _unauthorizedImageService;
    private readonly ImageUploadOptions _imageOptions;
    private readonly ILoggerFactory _loggerFactory;
}
