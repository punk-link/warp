namespace Warp.WebApp.Services;

public interface IViewCountService
{
    int AddAndGet(Guid itemId);
}