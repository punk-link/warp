namespace Warp.WebApp.Services;

public class ReportStorage : IReportStorage
{
    public ReportStorage()
    {
        _reportedEntries = [];
    }
    
    
    public void Add(Guid id)
        => _reportedEntries.Add(id);


    public bool Contains(Guid id)
        => _reportedEntries.Contains(id);


    private readonly HashSet<Guid> _reportedEntries;
}