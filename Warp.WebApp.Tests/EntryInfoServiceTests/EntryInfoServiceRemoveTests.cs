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

namespace Warp.WebApp.Tests.EntryInfoServiceTests;

public class EntryInfoServiceRemoveTests
{
    public EntryInfoServiceRemoveTests()
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
    public async Task Remove_ShouldThrowException_DataStorageRemoveFails()
    {
        var creator = new Creator(Guid.NewGuid());
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var entryInfoForSubstitute = new EntryInfo(Guid.NewGuid(), creator.Id, DateTime.Now, DateTime.Now.AddDays(1), EditMode.Simple,
            new Entry("Some content"), [], new EntryOpenGraphDescription("Some content", "Some content", null), 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<EntryInfo?>(entryInfoForSubstitute));

        _dataStorageSubstitute.Remove<EntryInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception()));

        await Assert.ThrowsAsync<Exception>(() => _entryInfoService.Remove(creator, entryId, cancellationToken));
    }


    [Fact]
    public async Task Remove_ShouldReturnProblemDetails_EntryDoesntBelongToCreator()
    {
        var creator = new Creator(Guid.NewGuid());
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var entryInfoForSubstitute = new EntryInfo(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, DateTime.Now.AddDays(1), EditMode.Simple,
            new Entry("Some content"), [], new EntryOpenGraphDescription("Some content", "Some content", null), 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<EntryInfo?>(entryInfoForSubstitute));

        var localizedString = new LocalizedString("NoPermissionErrorMessage", "Entry does not belong to creator.");
        _localizerSubstitute["NoPermissionErrorMessage"]
            .Returns(localizedString);

        var result = await _entryInfoService.Remove(creator, entryId, cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(localizedString.Value ,result.Error.Detail);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.Error.Status);
    }


    [Fact]
    public async Task Remove_ShouldReturnSuccess()
    {
        var creator = new Creator(Guid.NewGuid());
        var entryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var entryInfoForSubstitute = new EntryInfo(Guid.NewGuid(), creator.Id,DateTime.Now, DateTime.Now.AddDays(1), EditMode.Simple,
            new Entry("Some content"), [], new EntryOpenGraphDescription("Some content", "Some content", null), 0);

        _dataStorageSubstitute.TryGet<EntryInfo?>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<EntryInfo?>(entryInfoForSubstitute));

        _dataStorageSubstitute.Remove<EntryInfo>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await _entryInfoService.Remove(creator, entryId, cancellationToken);

        Assert.True(result.IsSuccess);
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
