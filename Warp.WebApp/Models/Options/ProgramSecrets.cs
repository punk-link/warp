using System.Text.Json.Serialization;

namespace Warp.WebApp.Models.Options;

public readonly record struct ProgramSecrets
{
    [JsonConstructor]
    public ProgramSecrets(string consulAddress, string consulToken)
    {
        ConsulAddress = consulAddress;
        ConsulToken = consulToken;
    }


    [JsonPropertyName("consul-address")]
    public string ConsulAddress { get; init; }
    [JsonPropertyName("consul-token")]
    public string ConsulToken { get; init; }
}