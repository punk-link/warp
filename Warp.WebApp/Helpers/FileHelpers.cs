using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Text;
using Warp.WebApp.Models.Files;

namespace Warp.WebApp.Helpers;

public static class FileHelpers
{
    public static async Task<Result<AppFile, ProblemDetails>> ProcessStreamedFile
    (
        MultipartSection section, 
        ContentDispositionHeaderValue contentDisposition, 
        string[] permittedExtensions, 
        long sizeLimit
    )
    {
        try
        {
            var fileName = contentDisposition.FileName.Value;
            if (string.IsNullOrEmpty(fileName))
                return Result.Failure<AppFile, ProblemDetails>(ProblemDetailsHelper.Create("The file is missing a name"));

            var memoryStream = new MemoryStream();
            await section.Body.CopyToAsync(memoryStream);

            if (memoryStream.Length is 0)
                return Result.Failure<AppFile, ProblemDetails>(ProblemDetailsHelper.Create("The file is empty"));
        
            if (memoryStream.Length > sizeLimit)
                return Result.Failure<AppFile, ProblemDetails>(ProblemDetailsHelper.Create("The file exceeds the size limit"));

            if(!IsValidFileExtensionAndSignature(contentDisposition.FileName.Value!, memoryStream, permittedExtensions))
                return Result.Failure<AppFile, ProblemDetails>(ProblemDetailsHelper.Create("The file type isn't permitted"));

            return new AppFile(memoryStream, section.ContentType!, contentDisposition.FileName.Value!);
        }
        catch (Exception ex)
        {
            // TODO: log exception one level up
            return Result.Failure<AppFile, ProblemDetails>(ProblemDetailsHelper.CreateServerException(ex.Message));
        }
    }


    private static bool IsValidFileExtensionAndSignature(string fileName, Stream data, string[] permittedExtensions)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !permittedExtensions.Contains(extension))
            return false;

        data.Position = 0;
        using var reader = new BinaryReader(data, Encoding.UTF8, leaveOpen: true);

        // TODO: temporary solution, add logs if this branch is taken
        // if no signatures are defined, any file with the correct extension is valid
        if (!_fileSignature.TryGetValue(extension, out var signatures))
            return true; 

        var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));
        data.Position = 0;

        return signatures.Any(signature => headerBytes.Take(signature.Length)
            .SequenceEqual(signature));
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
}
