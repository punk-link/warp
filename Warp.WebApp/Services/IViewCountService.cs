namespace Warp.WebApp.Services;

public interface IViewCountService
{
    public Task<long> AddAndGet(Guid itemId);
}