using CSharpFunctionalExtensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NSubstitute;
using Warp.WebApp.Data;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services.OpenGraph;

namespace Warp.WebApp.Tests;

public class OpenGraphServiceTests
{
    public OpenGraphServiceTests()
    {
        var localizerSubstitute = Substitute.For<IStringLocalizer<ServerResources>>();
        localizerSubstitute["Warplyn is a simple and secure way to share text and images."]
            .Returns(new LocalizedString("Warplyn is a simple and secure way to share text and images.", DefaultDescription));
        _localizer = localizerSubstitute;

        _optionsSnapshot = Substitute.For<IOptionsSnapshot<OpenGraphOptions>>();
        _options = new OpenGraphOptions
        {
            Title = OptionsTitle,
            DefaultImageUrl = new Uri(OptionsDefaultImageUrl)
        };
        _optionsSnapshot.Value.Returns(_options);

        _dataStorage = Substitute.For<IDataStorage>();
        _service = new OpenGraphService(_optionsSnapshot, _localizer, _dataStorage);
    }


    [Fact]
    public async Task Add_WithDescriptionSource_PersistsProcessedDescription()
    {
        var entryId = Guid.NewGuid();
        var expiresIn = TimeSpan.FromMinutes(5);
        EntryOpenGraphDescription? captured = null;
        _dataStorage.Set(Arg.Any<string>(), Arg.Do<EntryOpenGraphDescription>(d => captured = d), Arg.Is(expiresIn), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        const string descriptionSource = "This is a test description.";
        var previewImage = new Uri("https://example.com/image.jpg");

        var result = await _service.Add(entryId, descriptionSource, previewImage, expiresIn, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(captured);
        Assert.Equal(OptionsTitle, captured.Value.Title);
        Assert.Equal(descriptionSource, captured.Value.Description);
        Assert.Equal(previewImage, captured.Value.ImageUrl);
    }


    [Theory]
    [InlineData("This is a very long description that exceeds the maximum length allowed. It should be trimmed and end with a complete sentence. The rest of the text shouldn't appear.",
                "This is a very long description that exceeds the maximum length allowed. It should be trimmed and end with a complete sentence. The rest of the text shouldn't appear.")]
    [InlineData("This is a very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis, very long description that exceeds the maximum length allowed, it should be trimmed.",
                "This is a very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis, very long description that exceeds the maximum length allowed, it should...")]
    [InlineData("This is a very long description with no sentence breaks and the words just keep going on and on without any periods so it needs to find a good word boundary to trim at which is hard with such a long text without proper punctuation",
                "This is a very long description with no sentence breaks and the words just keep going on and on without any periods so it needs to find a good word boundary to trim at which is hard with such a...")]
    public async Task Add_WithLongDescription_TrimsAccordingToRules(string source, string expected)
    {
        var entryId = Guid.NewGuid();
        var expiresIn = TimeSpan.FromMinutes(5);
        EntryOpenGraphDescription? captured = null;
        _dataStorage.Set(Arg.Any<string>(), Arg.Do<EntryOpenGraphDescription>(d => captured = d), Arg.Is(expiresIn), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        await _service.Add(entryId, source, _defaultPreviewImage, expiresIn, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(expected, captured.Value.Description);
    }


    [Theory]
    [InlineData("This is a test description with <a href='https://example.com'>link</a>.", "This is a test description with link.")]
    [InlineData("This is a <strong>test</strong> description with <a href='https://example.com'>link</a>.", "This is a test description with link.")]
    [InlineData("<div class='content'><h1>Title</h1><p>First paragraph</p><p>Second with <br/> line break</p></div>", "Title First paragraph Second with line break")]
    public async Task Add_WithHtmlTags_StripsAndSanitizes(string source, string expected)
    {
        var entryId = Guid.NewGuid();
        var expiresIn = TimeSpan.FromMinutes(5);
        EntryOpenGraphDescription? captured = null;
        _dataStorage.Set(Arg.Any<string>(), Arg.Do<EntryOpenGraphDescription>(d => captured = d), Arg.Is(expiresIn), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        await _service.Add(entryId, source, _defaultPreviewImage, expiresIn, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(expected, captured.Value.Description);
    }


    [Fact]
    public async Task Add_WithHtmlEntities_DecodesAndNormalizes()
    {
        var entryId = Guid.NewGuid();
        var expiresIn = TimeSpan.FromMinutes(5);
        EntryOpenGraphDescription? captured = null;
        _dataStorage.Set(Arg.Any<string>(), Arg.Do<EntryOpenGraphDescription>(d => captured = d), Arg.Is(expiresIn), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        var source = "Special characters: &quot;quotes&quot;, &amp;ampersand, &lt;brackets&gt;";
        var expected = "Special characters: \"quotes\", &ampersand, <brackets>";

        await _service.Add(entryId, source, _defaultPreviewImage, expiresIn, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(expected, captured.Value.Description);
    }


    [Theory]
    [InlineData("Text with    multiple    spaces", "Text with multiple spaces")]
    [InlineData("Text with\u200B\u200Cinvisible\u200Dchars", "Text with invisible chars")]
    [InlineData("Text with\r\nnewlines\r\nand\ttabs", "Text with newlines and tabs")]
    [InlineData("<p>Text with\r\nnewlines\r\nand\ttabs</p>", "Text with newlines and tabs")]
    public async Task Add_WithSpecialCharacters_NormalizesWhitespace(string source, string expected)
    {
        var entryId = Guid.NewGuid();
        var expiresIn = TimeSpan.FromMinutes(5);
        EntryOpenGraphDescription? captured = null;
        _dataStorage.Set(Arg.Any<string>(), Arg.Do<EntryOpenGraphDescription>(d => captured = d), Arg.Is(expiresIn), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        await _service.Add(entryId, source, _defaultPreviewImage, expiresIn, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(expected, captured.Value.Description);
    }


    [Theory]
    [InlineData("<p>Cut text at the first 200 characters even if there's an incomplete HTML tag at the end like <strong>this one but it shouldn't appear in the final result because we strip HTML tags first", "Cut text at the first 200 characters even if there's an incomplete HTML tag at the end like this one but it shouldn't appear in the final result because we strip HTML tags first")]
    [InlineData("<div>Some text with unclosed div and <p>nested paragraph</div>", "Some text with unclosed div and nested paragraph")]
    public async Task Add_WithIncompleteHtml_ToleratesAndSanitizes(string source, string expected)
    {
        var entryId = Guid.NewGuid();
        var expiresIn = TimeSpan.FromMinutes(5);
        EntryOpenGraphDescription? captured = null;
        _dataStorage.Set(Arg.Any<string>(), Arg.Do<EntryOpenGraphDescription>(d => captured = d), Arg.Is(expiresIn), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        await _service.Add(entryId, source, _defaultPreviewImage, expiresIn, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(expected, captured.Value.Description);
    }


    [Fact]
    public async Task Add_WithOnlyHtml_FallsBackToDefaultDescription()
    {
        var entryId = Guid.NewGuid();
        var expiresIn = TimeSpan.FromMinutes(5);
        EntryOpenGraphDescription? captured = null;
        _dataStorage.Set(Arg.Any<string>(), Arg.Do<EntryOpenGraphDescription>(d => captured = d), Arg.Is(expiresIn), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        await _service.Add(entryId, "<div></div>", _defaultPreviewImage, expiresIn, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(DefaultDescription, captured.Value.Description);
    }


    [Fact]
    public async Task Add_WithProvidedDescriptionObject_StoresAsIs()
    {
        var entryId = Guid.NewGuid();
        var expiresIn = TimeSpan.FromMinutes(5);
        var description = new EntryOpenGraphDescription(OptionsTitle, "Custom", new Uri("https://example.com/custom.png"));
        EntryOpenGraphDescription? captured = null;
        _dataStorage.Set(Arg.Any<string>(), Arg.Do<EntryOpenGraphDescription>(d => captured = d), Arg.Is(expiresIn), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        var result = await _service.Add(entryId, description, expiresIn, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(captured);
        Assert.Equal(description, captured.Value);
    }


    [Fact]
    public async Task Add_WithRelativeOrNullImage_UsesDefault()
    {
        var entryId = Guid.NewGuid();
        var expiresIn = TimeSpan.FromMinutes(5);
        EntryOpenGraphDescription? capturedNull = null;
        _dataStorage.Set(Arg.Any<string>(), Arg.Do<EntryOpenGraphDescription>(d => capturedNull = d), Arg.Is(expiresIn), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        await _service.Add(entryId, "Some text", null, expiresIn, CancellationToken.None);
        Assert.NotNull(capturedNull);
        Assert.Equal(OptionsDefaultImageUrl, capturedNull!.Value.ImageUrl!.ToString());

        // Relative URL case
        EntryOpenGraphDescription? capturedRelative = null;
        _dataStorage.Set(Arg.Any<string>(), Arg.Do<EntryOpenGraphDescription>(d => capturedRelative = d), Arg.Is(expiresIn), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        await _service.Add(entryId, "Some text", new Uri("/relative.png", UriKind.Relative), expiresIn, CancellationToken.None);
        Assert.NotNull(capturedRelative);
        Assert.Equal(OptionsDefaultImageUrl, capturedRelative!.Value.ImageUrl!.ToString());
    }


    [Fact]
    public async Task Add_WithAbsoluteImage_KeepsImage()
    {
        var entryId = Guid.NewGuid();
        var expiresIn = TimeSpan.FromMinutes(5);
        var absolute = new Uri("https://example.com/image.png");
        EntryOpenGraphDescription? captured = null;
        _dataStorage.Set(Arg.Any<string>(), Arg.Do<EntryOpenGraphDescription>(d => captured = d), Arg.Is(expiresIn), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        await _service.Add(entryId, "Some text", absolute, expiresIn, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal(absolute, captured!.Value.ImageUrl);
    }


    [Fact]
    public async Task Get_CacheHit_ReturnsCachedDescription()
    {
        var entryId = Guid.NewGuid();
        var expected = new EntryOpenGraphDescription(OptionsTitle, "Cached", new Uri(OptionsDefaultImageUrl));
        _dataStorage.TryGet<EntryOpenGraphDescription?>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<EntryOpenGraphDescription?>(expected));

        var result = await _service.Get(entryId, CancellationToken.None);

        Assert.Equal(expected, result);
    }


    [Fact]
    public async Task Get_CacheMiss_ReturnsDefault()
    {
        var entryId = Guid.NewGuid();
        _dataStorage.TryGet<EntryOpenGraphDescription?>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<EntryOpenGraphDescription?>((EntryOpenGraphDescription?)null));

        var result = await _service.Get(entryId, CancellationToken.None);

        Assert.Equal(OptionsTitle, result.Title);
        Assert.Equal(DefaultDescription, result.Description);
        Assert.Equal(OptionsDefaultImageUrl, result.ImageUrl!.ToString());
    }


    [Fact]
    public void GetDefault_ReturnsLocalizedDefault_AndOptionsValues()
    {
        var result = _service.Get();

        Assert.Equal(OptionsTitle, result.Title);
        Assert.Equal(DefaultDescription, result.Description);
        Assert.Equal(OptionsDefaultImageUrl, result.ImageUrl!.ToString());
    }


    private const string DefaultDescription = "DefaultOpenGraphDescriptionText";
    private const string OptionsTitle = "CustomTitle";
    private const string OptionsDefaultImageUrl = "https://example.com/default-icon.png";
    private readonly Uri _defaultPreviewImage = new("https://example.com/image.jpg");

    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly IDataStorage _dataStorage;
    private readonly IOptionsSnapshot<OpenGraphOptions> _optionsSnapshot;
    private readonly OpenGraphOptions _options;
    private readonly OpenGraphService _service;
}