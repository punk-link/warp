using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries;
using Microsoft.Extensions.Localization;
using Moq;
using Warp.WebApp.Data;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.OpenGraph;
using Warp.WebApp.Services;
using Microsoft.Extensions.Logging;

namespace Warp.WebApp.Tests;
public class EntryInfoServiceTests
{
    public EntryInfoServiceTests()
    {
        _loggerMock = new();
        _openGraphServiceMock = new();
        _dataStorageMock = new();
        _reportServiceMock = new();
        _viewCountServiceMock = new();
        _imageServiceMock = new();
        _entryServiceMock = new();
        _creatorServiceMock = new();
        _localizerMock = new();

        _entryInfoService = new EntryInfoService(_creatorServiceMock.Object, _dataStorageMock.Object, _entryServiceMock.Object, _imageServiceMock.Object, _loggerMock.Object, _openGraphServiceMock.Object, _reportServiceMock.Object, _localizerMock.Object, _viewCountServiceMock.Object);
    }

    [Fact]
    public async Task Add_ShouldReturnEntryInfo_EntryIsAddedSuccessfully()
    {
        var creator = new Creator(Guid.NewGuid());
        var entryRequest = new EntryRequest() { Id = Guid.NewGuid(), ExpiresIn = TimeSpan.FromDays(1), EditMode = EditMode.Advanced, TextContent = "Test", ImageIds = new List<Guid>() { new Guid() } };
        var cancellationToken = CancellationToken.None;

        var entry = new Entry("Test");
        var imageInfos = new List<ImageInfo> { new ImageInfo { Id = Guid.NewGuid(), Url = new Uri("http://example.com/image.jpg") } };
        var entryInfo = new EntryInfo(entryRequest.Id, creator.Id, DateTime.UtcNow, DateTime.UtcNow + entryRequest.ExpiresIn, EditMode.Advanced, entry, imageInfos, new EntryOpenGraphDescription("Test", "Test", imageInfos[0].Url) , 0);

        _localizerMock.Setup(x => x["EntryExpirationPeriodEmptyErrorMessage"])
            .Returns(new LocalizedString("EntryExpirationPeriodEmptyErrorMessage", "Entry period is empty."));

        var imageUrl = imageInfos.Select(x => x.Url).FirstOrDefault();
        _openGraphServiceMock
            .Setup(x => x.BuildDescription(entry.Content, imageUrl))
            .Returns(new EntryOpenGraphDescription(entry.Content, entry.Content, imageUrl));

        _entryServiceMock
            .Setup(x => x.Add(entryRequest, cancellationToken))
            .ReturnsAsync(Result.Success<Entry, ProblemDetails>(entry));

        _imageServiceMock
            .Setup(x => x.GetAttached(entryRequest.Id, entryRequest.ImageIds, cancellationToken))
            .ReturnsAsync(Result.Success<List<ImageInfo>, ProblemDetails>((imageInfos)));


        _creatorServiceMock
            .Setup(x => x.AttachEntry(creator, entryInfo, cancellationToken))
            .ReturnsAsync(Result.Success<EntryInfo, ProblemDetails>(entryInfo));

        _dataStorageMock
            .Setup(x => x.Set(It.IsAny<string>(), entryInfo, entryRequest.ExpiresIn, cancellationToken))
            .ReturnsAsync(Result.Success());

        var result = await _entryInfoService.Add(creator, entryRequest, cancellationToken);


        var tolerance = TimeSpan.FromSeconds(5);

        Assert.Equal(result.Value.CreatedAt, entryInfo.CreatedAt, tolerance);
        Assert.Equal(result.Value.ExpiresAt, entryInfo.ExpiresAt, tolerance);

        Assert.Equal(result.Value.Id, entryInfo.Id);
        Assert.Equal(result.Value.CreatorId, entryInfo.CreatorId);
        Assert.Equal(result.Value.EditMode, entryInfo.EditMode);
        Assert.Equal(result.Value.Entry.Content, entryInfo.Entry.Content);
        Assert.Equal(result.Value.ImageInfos.Count, entryInfo.ImageInfos.Count);
        Assert.Equal(result.Value.OpenGraphDescription.Title, entryInfo.OpenGraphDescription.Title);
        Assert.Equal(result.Value.OpenGraphDescription.Description, entryInfo.OpenGraphDescription.Description);
        Assert.Equal(result.Value.OpenGraphDescription.ImageUrl, entryInfo.OpenGraphDescription.ImageUrl);
        Assert.Equal(result.Value.ViewCount, entryInfo.ViewCount);
    }

    [Fact]
    public async Task Add_ShouldReturnProblemDetails_EntryServiceFails()
    {
        var creator = new Creator(Guid.NewGuid());
        var entryRequest = new EntryRequest { Id = Guid.NewGuid(), ExpiresIn = TimeSpan.FromDays(1) };
        var cancellationToken = CancellationToken.None;

        var problemDetails = new ProblemDetails { Title = "Error", Detail = "Error" };

        _entryServiceMock
            .Setup(x => x.Add(entryRequest, cancellationToken))
            .ReturnsAsync(Result.Failure<Entry, ProblemDetails>(problemDetails));

        var result = await _entryInfoService.Add(creator, entryRequest, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, problemDetails);
    }

    private readonly IEntryInfoService _entryInfoService;
    private readonly Mock<ILoggerFactory> _loggerMock;
    private readonly Mock<IOpenGraphService> _openGraphServiceMock;
    private readonly Mock<IDataStorage> _dataStorageMock;
    private readonly Mock<IReportService> _reportServiceMock;
    private readonly Mock<IViewCountService> _viewCountServiceMock;
    private readonly Mock<IImageService> _imageServiceMock;
    private readonly Mock<IEntryService> _entryServiceMock;
    private readonly Mock<ICreatorService> _creatorServiceMock;
    private readonly Mock<IStringLocalizer<ServerResources>> _localizerMock;
}
