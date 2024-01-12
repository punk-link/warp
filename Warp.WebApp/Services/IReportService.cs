namespace Warp.WebApp.Services;

public interface IReportService
{
    public bool Contains(Guid id);
    
    public void MarkAsReported(Guid id);
}