using Newtonsoft.Json.Linq;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Helpers.Configuration;

public static class VaultHelper
{
    public static async Task<T> GetSecrets<T>(ILogger logger, IConfiguration configuration)
    {
        var vaultClient = GetVaultClient(configuration);
        object? response;
        try
        {
            response = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: configuration["ServiceName"], mountPoint: StorageName);
        }
        catch (Exception ex)
        {
            logger.LogVaultConnectionException(ex.Message);
            throw;
        }

        try
        {
            var jObject = JObject.FromObject(response!);
            return jObject.ToObject<T>()!;
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
