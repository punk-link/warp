namespace Warp.WebApp.Services.Entries;

public interface IViewCountService
{
    public Task<long> AddAndGet(Guid itemId);
}