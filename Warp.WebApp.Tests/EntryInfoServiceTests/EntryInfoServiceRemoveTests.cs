﻿using Microsoft.Extensions.Logging;
using NSubstitute;
using Warp.WebApp.Data;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.OpenGraph;

namespace Warp.WebApp.Tests.EntryInfoServiceTests;

public class EntryInfoServiceRemoveTests
{
    public EntryInfoServiceRemoveTests()
    {
        _loggerFactorySubstitute.CreateLogger<EntryInfoService>().Returns(_loggerSubstitute);
        
        _entryInfoService = new EntryInfoService(
            _creatorServiceSubstitute,
            _dataStorageSubstitute,
            _entryServiceSubstitute,
            _imageServiceSubstitute,
            _loggerFactorySubstitute,
            _openGraphServiceSubstitute,
            _reportServiceSubstitute,
            _viewCountServiceSubstitute
        );
        _creator = new Creator(Guid.NewGuid());
    }


    [Fact]
    public async Task Remove_ShouldThrowException_DataStorageRemoveFails()
    {
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var entryInfo = new EntryInfo(
            id: Guid.NewGuid(), 
            creatorId: _creator.Id, 
            createdAt: DateTime.UtcNow, 
            expiresAt: DateTime.UtcNow.AddDays(1), 
            editMode: EditMode.Simple,
            entry: new Entry("Some content"), 
            imageInfos: [], 
            viewCount: 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        _dataStorageSubstitute.Remove<EntryInfo>(Arg.Any<string>(), cancellationToken)
            .Returns(Task.FromException(new Exception()));

        await Assert.ThrowsAsync<Exception>(() => _entryInfoService.Remove(_creator, entryId, cancellationToken));
    }


    [Fact]
    public async Task Remove_ShouldReturnDomainError_EntryDoesntBelongToCreator()
    {
        var entryId = Guid.NewGuid();
        var differentCreatorId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var entryInfo = new EntryInfo(
            id: Guid.NewGuid(), 
            creatorId: differentCreatorId, 
            createdAt: DateTime.UtcNow, 
            expiresAt: DateTime.UtcNow.AddDays(1), 
            editMode: EditMode.Simple,
            entry: new Entry("Some content"), 
            imageInfos: [], 
            viewCount: 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        var result = await _entryInfoService.Remove(_creator, entryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(Constants.Logging.LogEvents.NoPermissionError, result.Error.Code);
    }


    [Fact]
    public async Task Remove_ShouldReturnDomainError_WhenEntryNotFound()
    {
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>((EntryInfo?)null));

        var result = await _entryInfoService.Remove(_creator, entryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(Constants.Logging.LogEvents.EntryNotFound, result.Error.Code);
    }


    [Fact]
    public async Task Remove_ShouldReturnSuccess_WhenEntryBelongsToCreator()
    {
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var entryInfo = new EntryInfo(
            id: Guid.NewGuid(), 
            creatorId: _creator.Id, 
            createdAt: DateTime.UtcNow, 
            expiresAt: DateTime.UtcNow.AddDays(1), 
            editMode: EditMode.Simple,
            entry: new Entry("Some content"), 
            imageInfos: [], 
            viewCount: 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), cancellationToken)
            .Returns(new ValueTask<EntryInfo?>(entryInfo));

        _dataStorageSubstitute.Remove<EntryInfo>(Arg.Any<string>(), cancellationToken)
            .Returns(Task.CompletedTask);

        var result = await _entryInfoService.Remove(_creator, entryId, cancellationToken);

        Assert.True(result.IsSuccess);
        await _dataStorageSubstitute.Received().Remove<EntryInfo>(Arg.Any<string>(), cancellationToken);
    }


    private readonly IEntryInfoService _entryInfoService;
    private readonly Creator _creator;
    private readonly ILoggerFactory _loggerFactorySubstitute = Substitute.For<ILoggerFactory>();
    private readonly ILogger<EntryInfoService> _loggerSubstitute = Substitute.For<ILogger<EntryInfoService>>();
    private readonly IOpenGraphService _openGraphServiceSubstitute = Substitute.For<IOpenGraphService>();
    private readonly IDataStorage _dataStorageSubstitute = Substitute.For<IDataStorage>();
    private readonly IReportService _reportServiceSubstitute = Substitute.For<IReportService>();
    private readonly IViewCountService _viewCountServiceSubstitute = Substitute.For<IViewCountService>();
    private readonly IImageService _imageServiceSubstitute = Substitute.For<IImageService>();
    private readonly IEntryService _entryServiceSubstitute = Substitute.For<IEntryService>();
    private readonly ICreatorService _creatorServiceSubstitute = Substitute.For<ICreatorService>();
}
