using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Models.Validators;

namespace Warp.WebApp.Tests.UnitTests;

public class EntryValidatorTests
{
    public EntryValidatorTests()
    {
        _options = new EntryValidatorOptions
        {
            MaxContentDeltaSizeBytes = 1024 * 1024,
            MaxHtmlSizeBytes = 1024 * 1024,
            MaxPlainTextSizeBytes = 1024 * 1024
        };
    }


    [Fact]
    public async Task Validate_SimpleMode_EmptyContent_Fails()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Simple, 
            TextContent = string.Empty,
            ImageIds = []
        };
        var entry = new Entry(content: string.Empty);
        var validator = new EntryValidator(entryRequest, _options);

        var result = await validator.ValidateAsync(entry, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }


    [Fact]
    public async Task Validate_SimpleMode_ValidContent_Succeeds()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Simple, 
            TextContent = "Valid text",
            ImageIds = []
        };
        var entry = new Entry(content: "Valid text");
        var validator = new EntryValidator(entryRequest, _options);

        var result = await validator.ValidateAsync(entry, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }


    [Fact]
    public async Task Validate_AdvancedMode_EmptyHtmlOnly_Fails()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Advanced, 
            TextContent = "<p><br></p>",
            ContentDelta = "{\"type\":\"doc\"}",
            ImageIds = []
        };
        var entry = new Entry(content: "<p><br></p>", contentDelta: "{\"type\":\"doc\"}");
        var validator = new EntryValidator(entryRequest, _options);

        var result = await validator.ValidateAsync(entry, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Content");
    }


    [Fact]
    public async Task Validate_AdvancedMode_EmptyParagraphs_Fails()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Advanced, 
            TextContent = "<p></p><p></p><p><br></p>",
            ContentDelta = "{\"type\":\"doc\"}",
            ImageIds = []
        };
        var entry = new Entry(content: "<p></p><p></p><p><br></p>", contentDelta: "{\"type\":\"doc\"}");
        var validator = new EntryValidator(entryRequest, _options);

        var result = await validator.ValidateAsync(entry, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }


    [Fact]
    public async Task Validate_AdvancedMode_OnlyWhitespace_Fails()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Advanced, 
            TextContent = "<p>   </p><p>\n\t</p>",
            ContentDelta = "{\"type\":\"doc\"}",
            ImageIds = []
        };
        var entry = new Entry(content: "<p>   </p><p>\n\t</p>", contentDelta: "{\"type\":\"doc\"}");
        var validator = new EntryValidator(entryRequest, _options);

        var result = await validator.ValidateAsync(entry, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }


    [Fact]
    public async Task Validate_AdvancedMode_ValidContent_Succeeds()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Advanced, 
            TextContent = "<p>Valid content</p>",
            ContentDelta = "{\"type\":\"doc\"}",
            ImageIds = []
        };
        var entry = new Entry(content: "<p>Valid content</p>", contentDelta: "{\"type\":\"doc\"}");
        var validator = new EntryValidator(entryRequest, _options);

        var result = await validator.ValidateAsync(entry, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }


    [Fact]
    public async Task Validate_AdvancedMode_WithImages_EmptyContentAllowed()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Advanced, 
            TextContent = "<p><br></p>",
            ContentDelta = "{\"type\":\"doc\"}",
            ImageIds = new List<Guid> { Guid.NewGuid() }
        };
        var entry = new Entry(content: "<p><br></p>", contentDelta: "{\"type\":\"doc\"}");
        var validator = new EntryValidator(entryRequest, _options);

        var result = await validator.ValidateAsync(entry, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }


    [Fact]
    public async Task Validate_AdvancedMode_MissingContentDelta_Fails()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Advanced, 
            TextContent = "<p>Content</p>",
            ContentDelta = null,
            ImageIds = []
        };
        var entry = new Entry(content: "<p>Content</p>", contentDelta: null);
        var validator = new EntryValidator(entryRequest, _options);

        var result = await validator.ValidateAsync(entry, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ContentDelta");
    }


    [Fact]
    public async Task Validate_AdvancedMode_InvalidJsonContentDelta_Fails()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Advanced, 
            TextContent = "<p>Content</p>",
            ContentDelta = "not valid json",
            ImageIds = []
        };
        var entry = new Entry(content: "<p>Content</p>", contentDelta: "not valid json");
        var validator = new EntryValidator(entryRequest, _options);

        var result = await validator.ValidateAsync(entry, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ContentDelta" && e.ErrorMessage.Contains("valid JSON"));
    }


    [Fact]
    public async Task Validate_AdvancedMode_ValidJsonContentDelta_Succeeds()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Advanced, 
            TextContent = "<p>Content</p>",
            ContentDelta = "{\"type\":\"doc\",\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Content\"}]}]}",
            ImageIds = []
        };
        var entry = new Entry(
            content: "<p>Content</p>", 
            contentDelta: "{\"type\":\"doc\",\"content\":[{\"type\":\"paragraph\",\"content\":[{\"type\":\"text\",\"text\":\"Content\"}]}]}"
        );
        var validator = new EntryValidator(entryRequest, _options);

        var result = await validator.ValidateAsync(entry, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }


    private readonly EntryValidatorOptions _options;
}
