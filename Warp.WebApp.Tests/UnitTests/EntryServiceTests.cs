using Microsoft.Extensions.Options;
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Tests.UnitTests;

public class EntryServiceTests
{
    public EntryServiceTests()
    {
        var options = new EntryValidatorOptions
        {
            MaxContentDeltaSizeBytes = 1024 * 1024,
            MaxHtmlSizeBytes = 1024 * 1024,
            MaxPlainTextSizeBytes = 1024 * 1024
        };
        var optionsWrapper = Options.Create(options);
        _entryService = new EntryService(optionsWrapper);
    }


    [Fact]
    public async Task Add_SimpleMode_ValidationFailure()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Simple, 
            TextContent = string.Empty,
            ImageIds = []
        };

        var result = await _entryService.Add(entryRequest, CancellationToken.None);
        
        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.EntryModelValidationError, result.Error.Code);
    }


    [Fact]
    public async Task Add_SimpleMode_ValidationSuccess()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Simple,
            TextContent = "Text",
            ImageIds = []
        };

        var result = await _entryService.Add(entryRequest, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(entryRequest.TextContent, result.Value.Content);
    }


    [Fact]
    public async Task Add_AdvancedMode_SanitizesHtml()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Advanced, 
            TextContent = "<p>Safe content</p><script>alert('xss')</script>",
            ContentDelta = "{\"type\":\"doc\"}",
            ImageIds = []
        };

        var result = await _entryService.Add(entryRequest, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("Safe content", result.Value.Content);
        Assert.DoesNotContain("<script>", result.Value.Content);
        Assert.DoesNotContain("alert", result.Value.Content);
    }


    [Fact]
    public async Task Add_AdvancedMode_RemovesMaliciousAttributes()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Advanced, 
            TextContent = "<p onclick=\"alert('xss')\">Text</p><a href=\"javascript:void(0)\">Link</a>",
            ContentDelta = "{\"type\":\"doc\"}",
            ImageIds = []
        };

        var result = await _entryService.Add(entryRequest, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain("onclick", result.Value.Content);
        Assert.DoesNotContain("javascript:", result.Value.Content);
        Assert.Contains("Text", result.Value.Content);
    }


    [Fact]
    public async Task Add_AdvancedMode_PreservesAllowedTags()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Advanced, 
            TextContent = "<h1>Title</h1><p>Text with <strong>bold</strong> and <em>italic</em></p><ul><li>Item</li></ul>",
            ContentDelta = "{\"type\":\"doc\"}",
            ImageIds = []
        };

        var result = await _entryService.Add(entryRequest, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("<h1>", result.Value.Content);
        Assert.Contains("<strong>", result.Value.Content);
        Assert.Contains("<em>", result.Value.Content);
        Assert.Contains("<ul>", result.Value.Content);
        Assert.Contains("<li>", result.Value.Content);
    }


    [Fact]
    public async Task Add_AdvancedMode_RequiresContentDelta()
    {
        var entryRequest = new EntryRequest 
        { 
            EditMode = EditMode.Advanced, 
            TextContent = "<p>Content</p>",
            ContentDelta = null,
            ImageIds = []
        };

        var result = await _entryService.Add(entryRequest, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.EntryModelValidationError, result.Error.Code);
    }


    private readonly EntryService _entryService;
}
