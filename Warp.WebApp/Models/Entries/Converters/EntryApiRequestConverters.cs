using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Services;

namespace Warp.WebApp.Models.Entries.Converters;

public static class EntryApiRequestConverters
{
    public static EntryRequest ToEntryRequest(this EntryApiRequest request, in Guid id)
    {
        return new EntryRequest
        {
            Id = id,
            EditMode = request.EditMode,
            ExpiresIn = GetExpirationPeriod(request.ExpirationPeriod),
            ImageIds = [.. request.ImageIds.Select(IdCoder.Decode)],
            TextContent = request.TextContent,
            ContentDelta = request.ContentDelta
        };


        static TimeSpan GetExpirationPeriod(ExpirationPeriod expirationPeriod)
            => expirationPeriod switch
            {
                ExpirationPeriod.FiveMinutes => new TimeSpan(0, 5, 0),
                ExpirationPeriod.ThirtyMinutes => new TimeSpan(0, 30, 0),
                ExpirationPeriod.OneHour => new TimeSpan(1, 0, 0),
                ExpirationPeriod.EightHours => new TimeSpan(8, 0, 0),
                ExpirationPeriod.OneDay => new TimeSpan(24, 0, 0),
                _ => new TimeSpan(0, 5, 0)
            };
    }
}
