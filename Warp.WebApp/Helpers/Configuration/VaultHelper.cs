using Microsoft.Extensions.Options;
using System.Text.Json;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Helpers.Configuration;

public static class VaultHelper
{
    public static async Task<T> GetSecrets<T>(ILogger logger, IConfiguration configuration)
    {
        var vaultClient = GetVaultClient(configuration);
        object? data;
        try
        {
            var response = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: configuration["ServiceName"], mountPoint: StorageName);
            data = response.Data.Data;
        }
        catch (Exception ex)
        {
            logger.LogVaultConnectionException(ex.Message);
            throw;
        }

        try
        {
            var rawData = new Dictionary<string, object?>();
            foreach (var kvp in (IDictionary<string, object?>)data!)
            {
                if (kvp.Value is JsonElement jsonElement)
                {
                    rawData[kvp.Key] = jsonElement.ValueKind switch
                    {
                        JsonValueKind.String => jsonElement.GetString(),
                        JsonValueKind.Number => 
                            jsonElement.TryGetInt64(out var l) ? l :
                            jsonElement.TryGetDecimal(out var d) ? d :
                            jsonElement.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Object => jsonElement.GetRawText(),
                        _ => jsonElement.GetRawText()
                    };
                }
                else
                {
                    rawData[kvp.Key] = kvp.Value;
                }
            }

            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(rawData, options);
            return JsonSerializer.Deserialize<T>(json, options)!;
        }
        catch (Exception ex)
        {
            logger.LogVaultSecretCastException(ex.Message);
            throw;
        }
    }


    /// <summary>
    /// Creates a Vault client using configuration values for address and token.
    /// </summary>
    /// <param name="configuration">The application configuration containing PNKL_VAULT_ADDR and PNKL_VAULT_TOKEN.</param>
    /// <returns>A configured <see cref="IVaultClient"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when PNKL_VAULT_ADDR or PNKL_VAULT_TOKEN is not configured or empty.</exception>
    public static IVaultClient GetVaultClient(IConfiguration configuration)
    {
        var vaultAddress = configuration["PNKL_VAULT_ADDR"];
        if (string.IsNullOrWhiteSpace(vaultAddress))
            throw new InvalidOperationException("PNKL_VAULT_ADDR is not configured or empty. Ensure the environment variable is set.");

        var vaultToken = configuration["PNKL_VAULT_TOKEN"];
        if (string.IsNullOrWhiteSpace(vaultToken))
            throw new InvalidOperationException("PNKL_VAULT_TOKEN is not configured or empty. For E2E tests, ensure PNKL_VAULT_TOKEN_FILE points to a valid token file.");

        var vaultClientSettings = new VaultClientSettings(vaultAddress, new TokenAuthMethodInfo(vaultToken));
        
        return new VaultClient(vaultClientSettings);
    }


    private const string StorageName = "secrets";
}
