using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Warp.WebApp.Data;
using Warp.WebApp.Data.Redis;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Images;
using Warp.WebApp.Models.Moderation;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services.Moderation;

namespace Warp.WebApp.Tests.UnitTests;

public class ModerationJobServiceTests
{
    public ModerationJobServiceTests()
    {
        _service = new ModerationJobService(_dataStorage, _indexStore, Options.Create(new ContentModerationOptions()));

        _dataStorage
            .Set(Arg.Any<string>(), Arg.Any<EntryModerationJob>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());

        _indexStore
            .Schedule(Arg.Any<Guid>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
    }


    [Fact]
    public async Task Schedule_ShouldPersistJob_WithImmediateNextAttempt()
    {
        EntryModerationJob? persistedJob = null;
        DateTimeOffset? scheduledAt = null;
        _dataStorage
            .Set(
                Arg.Any<string>(),
                Arg.Do<EntryModerationJob>(job => persistedJob = job),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());
        _indexStore
            .Schedule(Arg.Any<Guid>(), Arg.Do<DateTimeOffset>(value => scheduledAt = value), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var entryExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10);
        var entryInfo = BuildEntryInfo(entryExpiresAt);
        var startedAt = DateTimeOffset.UtcNow;

        await _service.Schedule(entryInfo, CancellationToken.None);

        var finishedAt = DateTimeOffset.UtcNow;

        Assert.NotNull(persistedJob);
        Assert.Equal(entryExpiresAt, persistedJob.ExpiresAt);
        Assert.InRange(persistedJob.NextAttemptAt, startedAt, finishedAt);
        Assert.Equal(persistedJob.NextAttemptAt, scheduledAt);
    }


    [Fact]
    public async Task Reschedule_ShouldPreserveEntryExpiration_AndUpdateNextAttempt()
    {
        EntryModerationJob? persistedJob = null;
        DateTimeOffset? scheduledAt = null;
        _dataStorage
            .Set(
                Arg.Any<string>(),
                Arg.Do<EntryModerationJob>(job => persistedJob = job),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(UnitResult.Success<DomainError>());
        _indexStore
            .Schedule(Arg.Any<Guid>(), Arg.Do<DateTimeOffset>(value => scheduledAt = value), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var entryExpiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var initialAttemptAt = DateTimeOffset.UtcNow;
        var retryAt = initialAttemptAt.AddMinutes(5);
        var job = EntryModerationJob.Create(Guid.NewGuid(), true, [Guid.NewGuid()], entryExpiresAt, initialAttemptAt);

        await _service.Reschedule(job, retryAt, CancellationToken.None);

        Assert.NotNull(persistedJob);
        Assert.Equal(entryExpiresAt, persistedJob.ExpiresAt);
        Assert.Equal(retryAt, persistedJob.NextAttemptAt);
        Assert.Equal(1, persistedJob.FailureCount);
        Assert.Equal(retryAt, scheduledAt);
    }


    private static EntryInfo BuildEntryInfo(DateTimeOffset expiresAt)
        => new(
            id: Guid.NewGuid(),
            creatorId: Guid.NewGuid(),
            createdAt: DateTimeOffset.UtcNow,
            expiresAt: expiresAt,
            editMode: EditMode.Simple,
            entry: new Entry("Text to moderate"),
            imageInfos: [new ImageInfo(id: Guid.NewGuid(), entryId: Guid.NewGuid(), url: new Uri("https://example.com/image.png"))],
            viewCount: 0);


    private readonly IDataStorage _dataStorage = Substitute.For<IDataStorage>();
    private readonly IModerationIndexStore _indexStore = Substitute.For<IModerationIndexStore>();
    private readonly ModerationJobService _service;
}