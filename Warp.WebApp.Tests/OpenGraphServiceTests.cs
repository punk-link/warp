using Microsoft.Extensions.Localization;
using NSubstitute;
using Warp.WebApp.Data;
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
        _dataStorage = Substitute.For<IDataStorage>();
    }


    [Fact]
    public void GetDefaultDescription_ReturnsDefaultDescription()
    {
        var service = new OpenGraphService(_localizer, _dataStorage);

        var result = service.GetDefaultDescription();

        Assert.NotEmpty(result.Title);
        Assert.Equal(DefaultDescription, result.Description);
        Assert.NotNull(result.ImageUrl);
    }


    [Fact]
    public void BuildDescription_WithDescription_ReturnsProcessedDescription()
    {
        var service = new OpenGraphService(_localizer, _dataStorage);

        var result = service.BuildDescription(Description, _defaultUri);

        Assert.NotEmpty(result.Title);
        Assert.Equal(Description, result.Description);
        Assert.NotNull(result.ImageUrl);
    }


    [Theory]
    [InlineData("This is a very long description that exceeds the maximum length allowed. It should be trimmed and end with a complete sentence. The rest of the text shouldn't appear.", "This is a very long description that exceeds the maximum length allowed. It should be trimmed and end with a complete sentence. The rest of the text shouldn't appear.")]
    [InlineData("This is a very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis, very long description that exceeds the maximum length allowed, it should be trimmed.", "This is a very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis, very long description that exceeds the maximum length allowed, it should...")]
    [InlineData("This is a very long description with no sentence breaks and the words just keep going on and on without any periods so it needs to find a good word boundary to trim at which is hard with such a long text without proper punctuation", "This is a very long description with no sentence breaks and the words just keep going on and on without any periods so it needs to find a good word boundary to trim at which is hard with such a...")]
    public void BuildDescription_WithLongDescription_ReturnsProcessedDescription(string description, string expected)
    {
        var service = new OpenGraphService(_localizer, _dataStorage);

        var result = service.BuildDescription(description, _defaultUri);

        Assert.Equal(expected, result.Description);
    }


    [Theory]
    [InlineData("This is a test description with <a href='https://example.com'>link</a>.", "This is a test description with link.")]
    [InlineData("This is a <strong>test</strong> description with <a href='https://example.com'>link</a>.", "This is a test description with link.")]
    [InlineData("<div class='content'><h1>Title</h1><p>First paragraph</p><p>Second with <br/> line break</p></div>", "Title First paragraph Second with line break")]
    public void BuildDescription_WithHtmlTags_ReturnsSanitizedDescription(string htmlDescription, string expected)
    {
        var service = new OpenGraphService(_localizer, _dataStorage);
        
        var result = service.BuildDescription(htmlDescription, _defaultUri);
        
        Assert.Equal(expected, result.Description);
    }


    [Fact]
    public void BuildDescription_WithHtmlEntities_ReturnsSanitizedDescription()
    {
        var service = new OpenGraphService(_localizer, _dataStorage);
        
        var htmlDescription = "Special characters: &quot;quotes&quot;, &amp;ampersand, &lt;brackets&gt;";
        var expected = "Special characters: \"quotes\", &ampersand, <brackets>";
        
        var result = service.BuildDescription(htmlDescription, _defaultUri);
        
        Assert.Equal(expected, result.Description);
    }


    [Theory]
    [InlineData("Text with    multiple    spaces", "Text with multiple spaces")]
    [InlineData("Text with\u200B\u200Cinvisible\u200Dchars", "Text with invisible chars")]
    [InlineData("Text with\r\nnewlines\r\nand\ttabs", "Text with newlines and tabs")]
    [InlineData("<p>Text with\r\nnewlines\r\nand\ttabs</p>", "Text with newlines and tabs")]
    public void BuildDescription_WithSpecialCharacters_ReturnsCleanedDescription(string input, string expected)
    {
        var service = new OpenGraphService(_localizer, _dataStorage);
        
        var result = service.BuildDescription(input, _defaultUri);
        
        Assert.Equal(expected, result.Description);
    }


    [Theory]
    [InlineData("<p>Cut text at the first 200 characters even if there's an incomplete HTML tag at the end like <strong>this one but it shouldn't appear in the final result because we strip HTML tags first", "Cut text at the first 200 characters even if there's an incomplete HTML tag at the end like this one but it shouldn't appear in the final result because we strip HTML tags first")]
    [InlineData("<div>Some text with unclosed div and <p>nested paragraph</div>", "Some text with unclosed div and nested paragraph")]
    public void BuildDescription_WithIncompleteHtmlTags_ReturnsSanitizedDescription(string htmlDescription, string expected)
    {
        var service = new OpenGraphService(_localizer, _dataStorage);
        
        var result = service.BuildDescription(htmlDescription, _defaultUri);
        
        Assert.Equal(expected, result.Description);
    }


    [Fact]
    public void BuildDescription_WithOnlyHtmlTags_ReturnsDefaultDescription()
    {
        var service = new OpenGraphService(_localizer, _dataStorage);
        
        var result = service.BuildDescription("<div></div>", _defaultUri);
        
        Assert.Equal(DefaultDescription, result.Description);
    }


    private const string DefaultDescription = "DefaultOpenGraphDescriptionText";
    private const string Description = "This is a test description.";
    private readonly Uri _defaultUri = new("https://example.com/image.jpg");

    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly IDataStorage _dataStorage;
}