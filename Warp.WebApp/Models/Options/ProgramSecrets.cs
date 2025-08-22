using System.Text.Json.Serialization;

namespace Warp.WebApp.Models.Options;

public readonly record struct ProgramSecrets
{
    [JsonConstructor]
    public ProgramSecrets(string consulAddress, string consulToken, string s3SecretAccessKey)
    {
        ConsulAddress = consulAddress;
        ConsulToken = consulToken;
        S3SecretAccessKey = s3SecretAccessKey;
    }


    [JsonPropertyName("consul-address")]
    public string ConsulAddress { get; init; }
    [JsonPropertyName("consul-token")]
    public string ConsulToken { get; init; }
    [JsonPropertyName("s3-secret-access-key")]
    public string S3SecretAccessKey { get; init; }
}