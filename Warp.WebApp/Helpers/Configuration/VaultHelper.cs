using Newtonsoft.Json.Linq;
using Vault;
using Vault.Client;
using Vault.Model;
using Warp.WebApp.Extensions.Logging;

namespace Warp.WebApp.Helpers.Configuration;

public static class VaultHelper
{
    public static T GetSecrets<T>(ILogger logger, IConfiguration configuration)
    {
        var vaultClient = GetVaultClient(configuration);
        VaultResponse<KvV2ReadResponse>? response;
        try
        {
            response = vaultClient.Secrets.KvV2Read(configuration["ServiceName"], StorageName);
        }
        catch (Exception ex)
        {
            logger.LogVaultConnectionException(ex.Message);
            throw;
        }

        try
        {
            var jObject = JObject.FromObject(response.Data.Data!);
            return jObject.ToObject<T>()!;
        }
        catch (Exception ex)
        {
            logger.LogVaultSecretCastException(ex.Message);
            throw;
        }
    }


    private static VaultClient GetVaultClient(IConfiguration configuration)
    {
        var vaultAddress = configuration["PNKL_VAULT_ADDR"]!;
        var vaultConfig = new VaultConfiguration(vaultAddress);

        var vaultClient = new VaultClient(vaultConfig);
        vaultClient.SetToken(configuration["PNKL_VAULT_TOKEN"]);

        return vaultClient;
    }


    private const string StorageName = "secrets";
}
