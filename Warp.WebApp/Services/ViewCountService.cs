namespace Warp.WebApp.Services;

public class ViewCountService : IViewCountService
{
    public int AddAndGet(Guid itemId)
    {
        lock (Counter)
        {
            if (!Counter.TryAdd(itemId, 1))
                Counter[itemId] += 1;

            return Counter[itemId];
        }
    }


    private static readonly Dictionary<Guid, int> Counter = [];
}