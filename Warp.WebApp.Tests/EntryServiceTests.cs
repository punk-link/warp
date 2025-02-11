using CSharpFunctionalExtensions;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;
using Moq;
using NSubstitute.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Files;
using Warp.WebApp.Models.Validators;
using Warp.WebApp.Services.Entries;

namespace Warp.WebApp.Tests;
public class EntryServiceTests
{
    [Fact]
    public async Task Add_ValidationSuccess()
    {
        var entryRequest = new EntryRequest { EditMode = Models.Entries.Enums.EditMode.Text, TextContent = "Text" };

        var mockLocalizer = new Mock<IStringLocalizer<ServerResources>>();
        mockLocalizer
            .Setup(l => l["EntryBodyEmptyErrorMessage"])
            .Returns(new LocalizedString("EntryBodyEmptyErrorMessage", "Entry body cannot be empty."));

        var entryService = new EntryService(mockLocalizer.Object);

        var result = await entryService.Add(entryRequest, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }
}
