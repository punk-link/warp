namespace Warp.WebApp.Services.Entries;

public interface IViewCountService
{
    public Task<long> AddAndGet(Guid itemId, CancellationToken cancellationToken);
    public Task<long> Get(Guid itemId, CancellationToken cancellationToken);
}