using System.Text.Json.Serialization;

namespace Warp.WebApp.Models.Options;

public readonly record struct ProgramSecrets
{
    [JsonConstructor]
    public ProgramSecrets(string consulAddress, string consulToken, string s3SecretAccessKey, string openAiModerationApiKey)
    {
        ConsulAddress = consulAddress;
        ConsulToken = consulToken;
        S3SecretAccessKey = s3SecretAccessKey;
        OpenAiModerationApiKey = openAiModerationApiKey;
    }


    [JsonPropertyName("consul-address")]
    public string ConsulAddress { get; init; }

    [JsonPropertyName("consul-token")]
    public string ConsulToken { get; init; }

    [JsonPropertyName("s3-secret-access-key")]
    public string S3SecretAccessKey { get; init; }

    [JsonPropertyName("openai-moderation-api-key")]
    public string OpenAiModerationApiKey { get; init; }
}