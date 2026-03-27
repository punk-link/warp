using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Helpers;

namespace Warp.WebApp.Tests.UnitTests.FileHelperTests;

public class FileHelperProcessStreamedFileTests
{
    public FileHelperProcessStreamedFileTests()
    {
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _loggerFactory.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger<FileHelper>>());
    }


    [Fact]
    public async Task ProcessStreamedFile_MissingFileName_ReturnsFailure()
    {
        var helper = CreateHelper();
        var (section, contentDisposition) = MakeSection(filename: null, content: PngBytes());

        var result = await helper.ProcessStreamedFile(section, contentDisposition);

        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.FileNameIsMissing, result.Error.Code);
    }


    [Fact]
    public async Task ProcessStreamedFile_EmptyFile_ReturnsFailure()
    {
        var helper = CreateHelper();
        var (section, contentDisposition) = MakeSection("test.png", []);

        var result = await helper.ProcessStreamedFile(section, contentDisposition);

        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.FileIsEmpty, result.Error.Code);
    }


    [Fact]
    public async Task ProcessStreamedFile_FileTooLarge_ReturnsFailure()
    {
        var helper = CreateHelper(fileSizeLimit: 4);
        var (section, contentDisposition) = MakeSection("test.png", PngBytes());

        var result = await helper.ProcessStreamedFile(section, contentDisposition);

        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.FileSizeExceeded, result.Error.Code);
    }


    [Fact]
    public async Task ProcessStreamedFile_SvgExtension_ReturnsFailure()
    {
        var helper = CreateHelper();
        var content = "<svg xmlns='http://www.w3.org/2000/svg'></svg>"u8.ToArray();
        var (section, contentDisposition) = MakeSection("image.svg", content);

        var result = await helper.ProcessStreamedFile(section, contentDisposition);

        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.FileTypeNotPermitted, result.Error.Code);
    }


    [Fact]
    public async Task ProcessStreamedFile_UnknownExtension_ReturnsFailure()
    {
        var helper = CreateHelper();
        var (section, contentDisposition) = MakeSection("binary.exe", [0x4D, 0x5A, 0x01, 0x02]);

        var result = await helper.ProcessStreamedFile(section, contentDisposition);

        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.FileTypeNotPermitted, result.Error.Code);
    }


    [Fact]
    public async Task ProcessStreamedFile_ValidPng_ReturnsDerivedMimeType()
    {
        var helper = CreateHelper();
        var (section, contentDisposition) = MakeSection("image.png", PngBytes());

        var result = await helper.ProcessStreamedFile(section, contentDisposition);

        Assert.True(result.IsSuccess);
        Assert.Equal("image/png", result.Value.ContentMimeType);
    }


    [Fact]
    public async Task ProcessStreamedFile_ValidJpeg_ReturnsDerivedMimeType()
    {
        var helper = CreateHelper();
        var (section, contentDisposition) = MakeSection("photo.jpg", JpegBytes());

        var result = await helper.ProcessStreamedFile(section, contentDisposition);

        Assert.True(result.IsSuccess);
        Assert.Equal("image/jpeg", result.Value.ContentMimeType);
    }


    [Fact]
    public async Task ProcessStreamedFile_ValidWebp_ReturnsDerivedMimeType()
    {
        var helper = CreateHelper();
        var (section, contentDisposition) = MakeSection("image.webp", WebpBytes());

        var result = await helper.ProcessStreamedFile(section, contentDisposition);

        Assert.True(result.IsSuccess);
        Assert.Equal("image/webp", result.Value.ContentMimeType);
    }


    [Fact]
    public async Task ProcessStreamedFile_WebpWithoutWebpMarker_ReturnsFailure()
    {
        var helper = CreateHelper();
        // RIFF header at bytes 0–3, but bytes 8–11 are "AVI " instead of "WEBP"
        byte[] content = [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x41, 0x56, 0x49, 0x20, 0x00];
        var (section, contentDisposition) = MakeSection("image.webp", content);

        var result = await helper.ProcessStreamedFile(section, contentDisposition);

        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.FileTypeNotPermitted, result.Error.Code);
    }


    private static (MultipartSection, ContentDispositionHeaderValue) MakeSection(string? filename, byte[] content)
    {
        var section = new MultipartSection { Body = new MemoryStream(content) };
        var disposition = filename is null
            ? "form-data; name=\"file\""
            : $"form-data; name=\"file\"; filename=\"{filename}\"";

        return (section, ContentDispositionHeaderValue.Parse(disposition));
    }


    private FileHelper CreateHelper(string[]? extensions = null, long? fileSizeLimit = null)
        => new(_loggerFactory, extensions ?? _defaultPermittedExtensions, fileSizeLimit ?? DefaultFileSizeLimit);


    private static byte[] PngBytes() => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];

    private static byte[] JpegBytes() => [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

    private static byte[] WebpBytes() => [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50, 0x00];

    private static readonly string[] _defaultPermittedExtensions = [".bmp", ".gif", ".ico", ".jpeg", ".jpg", ".png", ".tiff", ".webp"];
    private const long DefaultFileSizeLimit = 10 * 1024 * 1024;

    private readonly ILoggerFactory _loggerFactory;
}
