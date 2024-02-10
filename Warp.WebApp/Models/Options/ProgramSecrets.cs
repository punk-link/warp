using Newtonsoft.Json;

namespace Warp.WebApp.Models.Options;

public readonly record struct ProgramSecrets
{
    [JsonConstructor]
    public ProgramSecrets(string consulAddress, string consulToken)
    {
        ConsulAddress = consulAddress;
        ConsulToken = consulToken;
    }


    [JsonProperty("consul-address")]
    public string ConsulAddress { get; init; }
    [JsonProperty("consul-token")]
    public string ConsulToken { get; init; }
}