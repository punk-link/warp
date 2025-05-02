using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Models;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Tests;

public class EntryServiceTests
{
    [Fact]
    public async Task Add_ValidationFailure()
    {
        var entryRequest = new EntryRequest { EditMode = Models.Entries.Enums.EditMode.Simple, TextContent = string.Empty };
        var entryService = new EntryService();

        var result = await entryService.Add(entryRequest, CancellationToken.None);
        
        Assert.True(result.IsFailure);
        Assert.Equal(LogEvents.EntryModelValidationError, result.Error.Code);
    }


    [Fact]
    public async Task Add_ValidationSuccess()
    {
        var entryRequest = new EntryRequest { EditMode = Models.Entries.Enums.EditMode.Simple, TextContent = "Text" };
        var entryService = new EntryService();

        var result = await entryService.Add(entryRequest, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(entryRequest.TextContent, result.Value.Content);
    }
}
