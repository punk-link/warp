namespace Warp.WebApp.Services.Infrastructure;

public interface IUrlService
{
    Uri GetImageUrl(string decodedEntryId, in Guid imageId);
    Uri GetImageUrl(in Guid entryId, in Guid imageId);
}