using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Text;
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Helpers;

public class FileHelper
{
    public FileHelper(ILoggerFactory loggerFactory, string[] permittedExtensions, long fileSizeLimit)
    {
        _logger = loggerFactory.CreateLogger<FileHelper>();

        _permittedExtensions = permittedExtensions;
        _fileSizeLimit = fileSizeLimit;
    }


    public async Task<Result<AppFile, DomainError>> ProcessStreamedFile(MultipartSection section, ContentDispositionHeaderValue contentDisposition)
    {
        try
        {
            var fileName = contentDisposition.FileName.Value;
            if (string.IsNullOrEmpty(fileName))
                return new DomainError(LogEvents.FileNameIsMissing);

            var memoryStream = new MemoryStream();
            await section.Body.CopyToAsync(memoryStream);

            if (memoryStream.Length is 0)
                return new DomainError(LogEvents.FileIsEmpty);
        
            if (memoryStream.Length > _fileSizeLimit)
            {
                var logEvent = LogEvents.FileSizeExceeded;
                var description = string.Format(logEvent.ToDescriptionString(), memoryStream.Length, _fileSizeLimit);
                return new DomainError(logEvent, description);
            }

            if(!IsValidFileExtensionAndSignature(_logger, contentDisposition.FileName.Value!, memoryStream, _permittedExtensions))
                return new DomainError(LogEvents.FileTypeNotPermitted);

            return new AppFile(memoryStream, section.ContentType!, contentDisposition.FileName.Value!);
        }
        catch (Exception ex)
        {
            _logger.LogFileUploadException(ex.Message);

            var description = string.Format(LogEvents.ServerErrorWithMessage.ToDescriptionString(), ex);
            return new DomainError(LogEvents.ServerErrorWithMessage, description);
        }
    }


    private static bool IsValidFileExtensionAndSignature(ILogger<FileHelper> logger, string fileName, Stream data, string[] permittedExtensions)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
            return false;

        data.Position = 0;
        using var reader = new BinaryReader(data, Encoding.UTF8, leaveOpen: true);

        logger.LogUnverifiedFileSignatureError(extension);
        if (!_fileSignature.TryGetValue(extension, out var signatures))
            return true; 

        var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));
        data.Position = 0;

        var isValid = signatures.Any(signature => headerBytes.Take(signature.Length)
            .SequenceEqual(signature));
        if (!isValid)
            logger.LogFileSignatureVerificationError(extension, string.Join(", ", headerBytes));

        return isValid;
    }


    private static readonly Dictionary<string, List<byte[]>> _fileSignature = new()
    {
        [".bmp"] =
        [
            "BM"u8.ToArray()
        ],
        [".gif"] =
        [
            "GIF8"u8.ToArray()
        ],
        [".ico"] =
        [
            [0x00, 0x00, 0x01, 0x00]
        ],
        [".jpeg"] =
        [
            [0xFF, 0xD8, 0xFF, 0xDB],
            [0xFF, 0xD8, 0xFF, 0xE0],
            [0xFF, 0xD8, 0xFF, 0xE1],
            [0xFF, 0xD8, 0xFF, 0xEE]
        ],
        [".jpg"] =
        [
            [0xFF, 0xD8, 0xFF, 0xDB],
            [0xFF, 0xD8, 0xFF, 0xE0],
            [0xFF, 0xD8, 0xFF, 0xE1],
            [0xFF, 0xD8, 0xFF, 0xEE]
        ],
        [".png"] =
        [
            "‰PNG"u8.ToArray(),
            [137, 80, 78, 71, 13, 10, 26, 10]
        ],
        [".svg"] =
        [
            "<svg"u8.ToArray()
        ],
        [".tif"] =
        [
            [0x49, 0x49, 0x2A, 0x00],
            [0x4D, 0x4D, 0x00, 0x2A]
        ],
        [".tiff"] =
        [
            [0x49, 0x49, 0x2A, 0x00],
            [0x4D, 0x4D, 0x00, 0x2A]
        ],
        [".webp"] =
        [
            "RIFF"u8.ToArray()
        ]
    };

    
    private readonly long _fileSizeLimit;
    private readonly ILogger<FileHelper> _logger;
    private readonly string[] _permittedExtensions;
}
