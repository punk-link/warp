using System.Text.Json.Serialization;

namespace Warp.WebApp.Models.Creators;

public readonly record struct Creator
{
    [JsonConstructor]
    public Creator(Guid id)
    {
        Id = id;
    }


    public static Creator Empty 
        => new(Guid.Empty);


    public Guid Id { get; }
}
