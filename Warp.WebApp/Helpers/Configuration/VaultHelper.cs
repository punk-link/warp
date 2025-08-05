using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                        JsonValueKind.Number => jsonElement.GetDouble(),
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

            var json = JsonConvert.SerializeObject(rawData);
            return JsonConvert.DeserializeObject<T>(json)!;
        }
        catch (Exception ex)
        {
            logger.LogVaultSecretCastException(ex.Message);
            throw;
        }
    }


    public static IVaultClient GetVaultClient(IConfiguration configuration)
    {
        var vaultAddress = configuration["PNKL_VAULT_ADDR"]!;
        var vaultToken = configuration["PNKL_VAULT_TOKEN"]!;
        
        var vaultClientSettings = new VaultClientSettings(vaultAddress, new TokenAuthMethodInfo(vaultToken));
        
        return new VaultClient(vaultClientSettings);
    }


    private const string StorageName = "secrets";
}
