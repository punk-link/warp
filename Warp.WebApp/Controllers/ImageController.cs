using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Text;
using Warp.WebApp.Helpers.ImageUploadUtilities;
using Warp.WebApp.Models;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Images;

namespace Warp.WebApp.Controllers;

[ApiController]
[Route("/api/images")]
public sealed class ImageController : BaseController
{
    public ImageController(IImageService imageService)
    {
        _imageService = imageService;
    }


    [HttpGet("entry-id/{entryId}/image-id/{imageId}")]
    [OutputCache(Duration = 10 * 60, VaryByRouteValueNames = ["entryId", "imageId"])]
    public async Task<IActionResult> Get([FromRoute] string entryId, [FromRoute] string imageId)
    {
        var decodedEntryId = IdCoder.Decode(entryId);
        if (decodedEntryId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var decodedImageId = IdCoder.Decode(imageId);
        if (decodedImageId == Guid.Empty)
            return ReturnIdDecodingBadRequest();

        var (_, isFailure, value, error) = await _imageService.Get(decodedEntryId, decodedImageId);
        if (isFailure)
            return NotFound(error);

        return new FileStreamResult(new MemoryStream(value.Content), value.ContentType);
    }


    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] List<IFormFile> images)
    {
        var imageContainers = await _imageService.Add(images);
        var results = imageContainers.Select(x => new KeyValuePair<string, string>(x.Key, IdCoder.Encode(x.Value)))
            .ToList();

        return Ok(results);
    }


    [HttpPost]
    [DisableFormValueModelBinding]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadDatabase()
    {
        if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        {
            ModelState.AddModelError("File", $"The request couldn't be processed (Error 1).");
            // Log error

            return BadRequest(ModelState);
        }

        var formAccumulator = new KeyValueAccumulator();
        var trustedFileNameForDisplay = string.Empty;
        var untrustedFileNameForStorage = string.Empty;
        var streamedFileContent = Array.Empty<byte>();

        var boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(Request.ContentType),
            _defaultFormOptions.MultipartBoundaryLengthLimit);
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);

        var section = await reader.ReadNextSectionAsync();

        while (section != null)
        {
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(
                section.ContentDisposition, out var contentDisposition);

            if (hasContentDispositionHeader)
            {
                if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    untrustedFileNameForStorage = contentDisposition.FileName.Value;
                    trustedFileNameForDisplay = WebUtility.HtmlEncode(contentDisposition.FileName.Value);

                    streamedFileContent = await FileHelpers.ProcessStreamedFile(section, contentDisposition,
                        ModelState, _permittedExtensions, _fileSizeLimit);

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }
                }
                else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                {
                    var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
                    var encoding = GetEncoding(section);

                    if (encoding == null)
                    {
                        ModelState.AddModelError("File", $"The request couldn't be processed (Error 2).");
                        // Log error

                        return BadRequest(ModelState);
                    }

                    using (var streamReader = new StreamReader(
                        section.Body,
                        encoding,
                        detectEncodingFromByteOrderMarks: true,
                        bufferSize: 1024,
                        leaveOpen: true))
                    {
                        var value = await streamReader.ReadToEndAsync();

                        if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                        {
                            value = string.Empty;
                        }

                        formAccumulator.Append(key, value);

                        if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                        {
                            ModelState.AddModelError("File", $"The request couldn't be processed (Error 3).");
                            // Log error

                            return BadRequest(ModelState);
                        }
                    }
                }
            }

            section = await reader.ReadNextSectionAsync();
        }

        /*var formData = new FormData();
        var formValueProvider = new FormValueProvider(BindingSource.Form,
            new FormCollection(formAccumulator.GetResults()), CultureInfo.CurrentCulture);
        var bindingSuccessful = await TryUpdateModelAsync(formData, prefix: "",
            valueProvider: formValueProvider);

        if (!bindingSuccessful)
        {
            ModelState.AddModelError("File", "The request couldn't be processed (Error 5).");
            // Log error

            return BadRequest(ModelState);
        }*/

        /*var file = new AppFile()
        {
            Content = streamedFileContent,
            UntrustedName = untrustedFileNameForStorage,
            Note = formData.Note,
            Size = streamedFileContent.Length,
            UploadDT = DateTime.UtcNow
        };*/

        var imageInfo = new ImageInfo
        {
            Id = Guid.NewGuid(),
            Content = streamedFileContent,
            ContentType = Request.ContentType
        };

        await _imageService.Add(imageInfo);

        return Created(nameof(ImageController), null);
    }


    private static Encoding GetEncoding(MultipartSection section)
    {
        var hasMediaTypeHeader =
            MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

        // UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in 
        // most cases.
        if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
        {
            return Encoding.UTF8;
        }

        return mediaType.Encoding;
    }


    /*public class FormData
    {
        public string Note { get; set; }
    }*/


    private readonly IImageService _imageService;
    private readonly long _fileSizeLimit;
    private readonly string[] _permittedExtensions = { ".txt" };

    private static readonly FormOptions _defaultFormOptions = new FormOptions();
}