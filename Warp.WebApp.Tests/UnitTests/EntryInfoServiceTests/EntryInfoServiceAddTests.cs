using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Data;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.OpenGraph;
using Warp.WebApp.Telemetry.Metrics;

namespace Warp.WebApp.Tests.UnitTests.EntryInfoServiceTests;

public class EntryInfoServiceAddTests
{
    public EntryInfoServiceAddTests()
    {
        _loggerFactorySubstitute.CreateLogger<EntryInfoService>().Returns(_loggerSubstitute);
        
        _entryInfoService = new EntryInfoService(
            _creatorServiceSubstitute,
            _dataStorageSubstitute,
            _entryServiceSubstitute,
            _imageServiceSubstitute,
            _entryImageLifecycleServiceSubstitute,
            _loggerFactorySubstitute,
            _openGraphServiceSubstitute,
            _reportServiceSubstitute,
            _viewCountServiceSubstitute,
            _entryInfoMetricsSubstitute
        );
        _creator = new Creator(Guid.NewGuid());

        _entryImageLifecycleServiceSubstitute.Track(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }


    [Fact]
    public async Task Add_ShouldReturnEntryInfo_EntryIsAddedSuccessfully()
    {
        _entryInfoMetricsSubstitute.ClearReceivedCalls();

        var entryRequest = new EntryRequest
        {
            Id = Guid.NewGuid(),
            ExpiresIn = TimeSpan.FromDays(1),
            EditMode = EditMode.Advanced,
            TextContent = "Test",
            ImageIds = [Guid.NewGuid()]
        };
        var cancellationToken = CancellationToken.None;

        var entry = new Entry("Test");
        var imageInfos = new List<ImageInfo>
        {
            new(id: Guid.NewGuid(), entryId: entryRequest.Id, url: new Uri("http://example.com/image.jpg"))
        };
        var entryInfo = new EntryInfo(entryRequest.Id, _creator.Id, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow + entryRequest.ExpiresIn, 
            EditMode.Advanced, entry, imageInfos, 0);

        _entryServiceSubstitute.Add(entryRequest, cancellationToken)
            .Returns(Result.Success<Entry, DomainError>(entry));

        _imageServiceSubstitute.GetAttached(entryRequest.Id, entryRequest.ImageIds, cancellationToken)
            .Returns(Result.Success<List<ImageInfo>, DomainError>(imageInfos));

        _openGraphServiceSubstitute.Add(entryRequest.Id, entry.Content, imageInfos[0].Url, entryRequest.ExpiresIn, cancellationToken)
            .Returns(UnitResult.Success<DomainError>());

        _creatorServiceSubstitute.AttachEntry(_creator, Arg.Any<EntryInfo>(), cancellationToken)
            .Returns(Result.Success<EntryInfo, DomainError>(entryInfo));

        _dataStorageSubstitute.Set(Arg.Any<string>(), entryInfo, entryRequest.ExpiresIn, cancellationToken)
            .Returns(UnitResult.Success<DomainError>());

        var result = await _entryInfoService.Add(_creator, entryRequest, cancellationToken);

        var tolerance = TimeSpan.FromSeconds(5);

        Assert.Equal(result.Value.CreatedAt, entryInfo.CreatedAt, tolerance);
        Assert.Equal(result.Value.ExpiresAt, entryInfo.ExpiresAt, tolerance);
        Assert.Equal(result.Value.Id, entryInfo.Id);
        Assert.Equal(result.Value.CreatorId, entryInfo.CreatorId);
        Assert.Equal(result.Value.EditMode, entryInfo.EditMode);
        Assert.Equal(result.Value.Entry.Content, entryInfo.Entry.Content);
        Assert.Equal(result.Value.ImageInfos.Count, entryInfo.ImageInfos.Count);
        Assert.Equal(result.Value.ViewCount, entryInfo.ViewCount);

        await _openGraphServiceSubstitute.Received(1)
            .Add(entryRequest.Id, entry.Content, imageInfos[0].Url, entryRequest.ExpiresIn, cancellationToken);

        _entryInfoMetricsSubstitute.Received(1)
            .TrackActionCompleted(
                EntryInfoMetricActions.Add,
                EntryInfoMetricOutcomes.Success,
                Arg.Any<TimeSpan>(),
                Arg.Is<LogEvents?>(reason => reason == null));
    }


    [Fact]
    public async Task Add_ShouldReturnDomainError_EntryServiceFails()
    {
        _entryInfoMetricsSubstitute.ClearReceivedCalls();

        var entryRequest = new EntryRequest { Id = Guid.NewGuid(), ExpiresIn = TimeSpan.FromDays(1) };
        var cancellationToken = CancellationToken.None;

        var domainError = DomainErrors.EntryModelValidationError();

        _entryServiceSubstitute.Add(entryRequest, cancellationToken)
            .Returns(Result.Failure<Entry, DomainError>(domainError));

        var result = await _entryInfoService.Add(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(domainError.Code, result.Error.Code);
        Assert.Equal(domainError.Detail, result.Error.Detail);

        _entryInfoMetricsSubstitute.Received(1)
            .TrackActionCompleted(
                EntryInfoMetricActions.Add,
                EntryInfoMetricOutcomes.Failure,
                Arg.Any<TimeSpan>(),
                Arg.Is<LogEvents?>(reason => reason == domainError.Code));
    }


    [Fact]
    public async Task Add_ShouldReturnDomainError_GetImageInfosFails()
    {
        _entryInfoMetricsSubstitute.ClearReceivedCalls();

        var entryRequest = new EntryRequest
        {
            Id = Guid.NewGuid(),
            ExpiresIn = TimeSpan.FromDays(1),
            ImageIds = new List<Guid> { Guid.NewGuid() }
        };
        var cancellationToken = CancellationToken.None;

        var entry = new Entry("Test");
        var domainError = DomainErrors.FileUploadException("Failed to get image info");

        _entryServiceSubstitute.Add(entryRequest, cancellationToken)
            .Returns(Result.Success<Entry, DomainError>(entry));

        _imageServiceSubstitute.GetAttached(entryRequest.Id, entryRequest.ImageIds, cancellationToken)
            .Returns(Result.Failure<List<ImageInfo>, DomainError>(domainError));

        var result = await _entryInfoService.Add(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(domainError.Code, result.Error.Code);
        Assert.Equal(domainError.Detail, result.Error.Detail);
    }


    [Fact]
    public async Task Add_ShouldReturnDomainError_AttachToCreatorFails()
    {
        _entryInfoMetricsSubstitute.ClearReceivedCalls();

        var entryRequest = new EntryRequest
        {
            Id = Guid.NewGuid(),
            ExpiresIn = TimeSpan.FromDays(1),
            EditMode = EditMode.Simple,
            TextContent = "Test"
        };
        var cancellationToken = CancellationToken.None;

        var entry = new Entry("Test");
        var imageInfos = new List<ImageInfo>();
        var domainError = DomainErrors.CantAttachEntryToCreator();

        _entryServiceSubstitute.Add(entryRequest, cancellationToken)
            .Returns(Result.Success<Entry, DomainError>(entry));

        _imageServiceSubstitute.GetAttached(entryRequest.Id, entryRequest.ImageIds, cancellationToken)
            .Returns(Result.Success<List<ImageInfo>, DomainError>(imageInfos));

        _openGraphServiceSubstitute.Add(entryRequest.Id, entry.Content, null, entryRequest.ExpiresIn, cancellationToken)
            .Returns(UnitResult.Success<DomainError>());

        _creatorServiceSubstitute.AttachEntry(Arg.Any<Creator>(), Arg.Any<EntryInfo>(), cancellationToken)
            .Returns(Result.Failure<EntryInfo, DomainError>(domainError));

        var result = await _entryInfoService.Add(_creator, entryRequest, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(domainError.Code, result.Error.Code);
        Assert.Equal(domainError.Detail, result.Error.Detail);
    }


    private readonly EntryInfoService _entryInfoService;
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
    private readonly IEntryImageLifecycleService _entryImageLifecycleServiceSubstitute = Substitute.For<IEntryImageLifecycleService>();
    private readonly IEntryInfoMetrics _entryInfoMetricsSubstitute = Substitute.For<IEntryInfoMetrics>();
}
