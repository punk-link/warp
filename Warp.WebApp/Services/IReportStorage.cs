namespace Warp.WebApp.Services;

public interface IReportStorage
{
    public void Add(Guid id);

    public bool Contains(Guid id);
}