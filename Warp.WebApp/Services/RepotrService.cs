namespace Warp.WebApp.Services;

public class ReportService : IReportService
{
    public ReportService(IReportStorage reportStorage)
    {
        _reportStorage = reportStorage;
    }


    public bool Contains(Guid id)
    {
        return _reportStorage.Contains(id);
    }
    
    
    public void MarkAsReported(Guid id)
    {
        _reportStorage.Add(id);
    }


    private readonly IReportStorage _reportStorage;
}