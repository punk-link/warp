using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Net;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Data;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.OpenGraph;
using Warp.WebApp.Services;
using Microsoft.Extensions.Logging;
using Warp.WebApp.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Warp.WebApp.Tests;

public class EntryInfoServiceTests
{
    public EntryInfoServiceTests()
    {
        _entryInfoService = new EntryInfoService(
            _creatorServiceSubstitute,
            _dataStorageSubstitute,
            _entryServiceSubstitute,
            _imageServiceSubstitute,
            _loggerSubstitute,
            _openGraphServiceSubstitute,
            _reportServiceSubstitute,
            _localizerSubstitute,
            _viewCountServiceSubstitute
        );
        _creator = new Creator(Guid.NewGuid());
    }

    [Fact]
    public async Task Add_ShouldReturnEntryInfo_EntryIsAddedSuccessfully()
    {
        var entryRequest = new EntryRequest
        {
            Id = Guid.NewGuid(),
            ExpiresIn = TimeSpan.FromDays(1),
            EditMode = EditMode.Advanced,
            TextContent = "Test",
            ImageIds = new List<Guid> { Guid.NewGuid() }
        };
        var cancellationToken = CancellationToken.None;

        var entry = new Entry("Test");
        var imageInfos = new List<ImageInfo>
        {
            new ImageInfo { Id = Guid.NewGuid(), Url = new Uri("http://example.com/image.jpg") }
        };
        var entryInfo = new EntryInfo(
            entryRequest.Id,
            _creator.Id,
            DateTime.UtcNow,
            DateTime.UtcNow + entryRequest.ExpiresIn,
            EditMode.Advanced,
            entry,
            imageInfos,
            new EntryOpenGraphDescription("Test", "Test", imageInfos[0].Url),
            0
        );

        _localizerSubstitute["EntryExpirationPeriodEmptyErrorMessage"]
            .Returns(new LocalizedString("EntryExpirationPeriodEmptyErrorMessage", "Entry period is empty."));

        var imageUrl = imageInfos.Select(x => x.Url).FirstOrDefault();
        _openGraphServiceSubstitute
            .BuildDescription(entry.Content, imageUrl)
            .Returns(new EntryOpenGraphDescription(entry.Content, entry.Content, imageUrl));

        _entryServiceSubstitute
            .Add(entryRequest, cancellationToken)
            .Returns(Result.Success<Entry, ProblemDetails>(entry));

        _imageServiceSubstitute
            .GetAttached(entryRequest.Id, entryRequest.ImageIds, cancellationToken)
            .Returns(Result.Success<List<ImageInfo>, ProblemDetails>(imageInfos));

        _creatorServiceSubstitute
            .AttachEntry(_creator, entryInfo, cancellationToken)
            .Returns(Result.Success<EntryInfo, ProblemDetails>(entryInfo));

        _dataStorageSubstitute
            .Set(Arg.Any<string>(), entryInfo, entryRequest.ExpiresIn, cancellationToken)
            .Returns(Result.Success());

        var result = await _entryInfoService.Add(_creator, entryRequest, cancellationToken);

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
        var entryRequest = new EntryRequest { Id = Guid.NewGuid(), ExpiresIn = TimeSpan.FromDays(1) };
        var cancellationToken = CancellationToken.None;

        var problemDetails = new ProblemDetails { Title = "Error", Detail = "Error" };

        _entryServiceSubstitute
            .Add(entryRequest, cancellationToken)
            .Returns(Result.Failure<Entry, ProblemDetails>(problemDetails));

        var result = await _entryInfoService.Add(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, problemDetails);
    }

    [Fact]
    public async Task Remove_ShouldThrowException_DataStorageRemoveFails()
    {
        var creator = new Creator(Guid.NewGuid());
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var entryInfoForSubstitute = new EntryInfo(
            Guid.NewGuid(),
            creator.Id,
            DateTime.Now,
            DateTime.Now.AddDays(1),
            EditMode.Text,
            new Entry("Some content"),
            new List<ImageInfo>(),
            new EntryOpenGraphDescription("Some content", "Some content", null),
            0);

        _dataStorageSubstitute
            .TryGet<EntryInfo?>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<EntryInfo?>(entryInfoForSubstitute));

        _dataStorageSubstitute
            .Remove<EntryInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception()));

        await Assert.ThrowsAsync<Exception>(() => _entryInfoService.Remove(creator, entryId, cancellationToken));
    }

    [Fact]
    public async Task Remove_ShouldReturnSuccess()
    {
        var creator = new Creator(Guid.NewGuid());
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var entryInfoForSubstitute = new EntryInfo(
            Guid.NewGuid(),
            creator.Id,
            DateTime.Now,
            DateTime.Now.AddDays(1),
            EditMode.Text,
            new Entry("Some content"),
            new List<ImageInfo>(),
            new EntryOpenGraphDescription("Some content", "Some content", null),
            0);

        _dataStorageSubstitute
            .TryGet<EntryInfo?>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<EntryInfo?>(entryInfoForSubstitute));

        _dataStorageSubstitute
            .Remove<EntryInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _entryInfoService.Remove(creator, entryId, cancellationToken);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Remove_ShouldReturnProblemDetails_EntryDoesntBelongToCreator()
    {
        var creator = new Creator(Guid.NewGuid());
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var entryInfoForSubstitute = new EntryInfo(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.Now,
            DateTime.Now.AddDays(1),
            EditMode.Text,
            new Entry("Some content"),
            new List<ImageInfo>(),
            new EntryOpenGraphDescription("Some content", "Some content", null),
            0);

        _dataStorageSubstitute
            .TryGet<EntryInfo?>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<EntryInfo?>(entryInfoForSubstitute));

        var localizedString = new LocalizedString("NoPermissionErrorMessage", "Entry does not belong to creator.");
        _localizerSubstitute["NoPermissionErrorMessage"]
            .Returns(localizedString);

        var result = await _entryInfoService.Remove(creator, entryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(localizedString.Value ,result.Error.Detail);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }



    private readonly IEntryInfoService _entryInfoService;
    private readonly Creator _creator;
    private readonly ILoggerFactory _loggerSubstitute = Substitute.For<ILoggerFactory>();
    private readonly IOpenGraphService _openGraphServiceSubstitute = Substitute.For<IOpenGraphService>();
    private readonly IDataStorage _dataStorageSubstitute = Substitute.For<IDataStorage>();
    private readonly IReportService _reportServiceSubstitute = Substitute.For<IReportService>();
    private readonly IViewCountService _viewCountServiceSubstitute = Substitute.For<IViewCountService>();
    private readonly IImageService _imageServiceSubstitute = Substitute.For<IImageService>();
    private readonly IEntryService _entryServiceSubstitute = Substitute.For<IEntryService>();
    private readonly ICreatorService _creatorServiceSubstitute = Substitute.For<ICreatorService>();
    private readonly IStringLocalizer<ServerResources> _localizerSubstitute = Substitute.For<IStringLocalizer<ServerResources>>();
}
