namespace Warp.WebApp.Services;

public interface IReportService
{
    public ValueTask<bool> Contains(Guid id);
    
    public Task MarkAsReported(Guid id);
}