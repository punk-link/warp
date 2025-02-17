using CSharpFunctionalExtensions;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;
using Moq;
using NSubstitute;
using NSubstitute.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        var substituteLocalizer = Substitute.For<IStringLocalizer<ServerResources>>();
        substituteLocalizer["EntryBodyEmptyErrorMessage"]
            .Returns(new LocalizedString("EntryBodyEmptyErrorMessage", "Entry body cannot be empty."));

        var entryService = new EntryService(substituteLocalizer);

        var result = await entryService.Add(entryRequest, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task Add_ValidationFailure()
    {
        var entryRequest = new EntryRequest { EditMode = Models.Entries.Enums.EditMode.Text, TextContent = string.Empty };

        var substituteLocalizer = Substitute.For<IStringLocalizer<ServerResources>>();
        substituteLocalizer["EntryBodyEmptyErrorMessage"]
            .Returns(new LocalizedString("EntryBodyEmptyErrorMessage", "Entry body cannot be empty."));

        var entryService = new EntryService(substituteLocalizer);

        var result = await entryService.Add(entryRequest, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }
}
