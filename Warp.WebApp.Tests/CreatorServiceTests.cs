using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Localization;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warp.WebApp.Data;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Services.Creators;

namespace Warp.WebApp.Tests;
public class CreatorServiceTests
{
    public CreatorServiceTests()
    {    
        _creatorService = new CreatorService(_localizerMock.Object, _dataStorageMock.Object);
    }

    [Fact]
    public async Task Add_Default()
    {
        var creator = await _creatorService.Add(CancellationToken.None);
        Assert.NotNull<Creator>(creator);
    }

    [Fact]
    public async Task Get_CreatorIdNotFound_ReturnsProblemDetails()
    {
        var creatorId = Guid.NewGuid();

        _dataStorageMock
            .Setup(x => x.TryGet<Creator>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Creator));

        var result = await _creatorService.Get(creatorId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(400, result.Error.Status);
    }

    [Fact]
    public async Task Get_CreatorIdIsNull_ReturnsProblemDetails()
    {
        var result = await _creatorService.Get(null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(400, result.Error.Status);
    }

    private readonly Mock<IDataStorage> _dataStorageMock = new();
    private readonly Mock<IStringLocalizer<ServerResources>> _localizerMock = new();
    private readonly CreatorService _creatorService;
}
