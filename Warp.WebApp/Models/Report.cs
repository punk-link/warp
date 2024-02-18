namespace Warp.WebApp.Models;

public readonly record struct Report
{
    public Report(in Guid id)
    {
        Id = id;
    }


    public Guid Id { get; init; }
}