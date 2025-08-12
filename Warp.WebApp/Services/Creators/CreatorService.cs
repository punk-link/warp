using CSharpFunctionalExtensions;
using Warp.WebApp.Attributes;
using Warp.WebApp.Data;
using Warp.WebApp.Models.Creators;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Services.Creators;

/// <summary>
/// Implements functionality for managing creators within the application.
/// </summary>
public class CreatorService : ICreatorService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatorService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The factory used to create loggers.</param>
    /// <param name="dataStorage">The data storage service for persisting creator information.</param>
    public CreatorService(ILoggerFactory loggerFactory, IDataStorage dataStorage)
    {
        _dataStorage = dataStorage;
        _logger = loggerFactory.CreateLogger<CreatorService>();
    }


    /// <inheritdoc cref="ICreatorService.Add"/>
    [TraceMethod]
    public async Task<Creator> Add(CancellationToken cancellationToken)
    {
        var creatorId = Guid.CreateVersion7();
        var creator = new Creator(creatorId);
        
        var userCacheKey = CacheKeyBuilder.BuildCreatorCacheKey(creatorId);
        await _dataStorage.Set(userCacheKey, creator, TimeSpan.FromDays(365), cancellationToken);
        
        return creator;
    }


    /// <inheritdoc cref="ICreatorService.AttachEntry"/>
    [TraceMethod]
    public async Task<UnitResult<DomainError>> AttachEntry(Creator creator, EntryInfo entryInfo, CancellationToken cancellationToken)
    {
        var userIdCacheKey = CacheKeyBuilder.BuildCreatorsEntrySetCacheKey(creator.Id);

        return await UnitResult.Success<DomainError>()
            .Map(GetMaxExpirationDateTime)
            .Bind(GetExpirationSpan)
            .Bind(AttachEntryToCreator);


        async Task<DateTime> GetMaxExpirationDateTime()
        {
            var entryInfoIds = await _dataStorage.TryGetSet<Guid>(userIdCacheKey, cancellationToken);
            if (entryInfoIds.Count == 0)
                return DateTime.MinValue;

            var expirationDates = new List<DateTime>(entryInfoIds.Count);
            foreach (var entryInfoId in entryInfoIds)
            {
                var entryIdCacheKey = CacheKeyBuilder.BuildEntryInfoCacheKey(entryInfoId);

                // TODO: requesting all entries one by one is suboptimal. Consider refactoring.
                var entryInfo = await _dataStorage.TryGet<EntryInfo>(entryIdCacheKey, cancellationToken);
                expirationDates.Add(entryInfo.ExpiresAt);
            }

            return expirationDates.Max();
        }


        Result<TimeSpan, DomainError> GetExpirationSpan(DateTime maxExpirationDateTime) 
            => maxExpirationDateTime > entryInfo.ExpiresAt
                ? maxExpirationDateTime - entryInfo.ExpiresAt
                : entryInfo.ExpiresAt - maxExpirationDateTime;


        async Task<UnitResult<DomainError>> AttachEntryToCreator(TimeSpan expirationSpan)
        {
            var result = await _dataStorage.AddToSet(userIdCacheKey, entryInfo.Id, expirationSpan, cancellationToken);
            if (result.IsSuccess)
                return UnitResult.Success<DomainError>();

            _logger.LogCantAttachEntryToCreator(entryInfo.Id, creator.Id);

            return DomainErrors.CantAttachEntryToCreator();
        }
    }


    /// <inheritdoc cref="ICreatorService.Get"/>
    [TraceMethod]
    public async Task<Result<Creator, DomainError>> Get(Guid? creatorId, CancellationToken cancellationToken)
    {
        if (creatorId is null)
        {
            _logger.LogCreatorIdIsNull();
            return DomainErrors.CreatorIdIsNull();
        }

        var userCacheKey = CacheKeyBuilder.BuildCreatorCacheKey(creatorId.Value);
        var creator = await _dataStorage.TryGet<Creator>(userCacheKey, cancellationToken);
        if (creator == default)
            return DomainErrors.CreatorIdIsNotFound();

        return creator;
    }


    private readonly IDataStorage _dataStorage;
    private readonly ILogger<CreatorService> _logger;
}