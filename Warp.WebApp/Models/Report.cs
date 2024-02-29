using System.Text.Json.Serialization;

namespace Warp.WebApp.Models;

public readonly record struct Report
{
    [JsonConstructor]
    public Report(Guid id)
    {
        Id = id;
    }


    public Guid Id { get; }
}