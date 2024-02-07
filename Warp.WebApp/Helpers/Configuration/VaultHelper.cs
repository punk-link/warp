using Vault;
using Vault.Client;

namespace Warp.WebApp.Helpers.Configuration;

public static class VaultHelper
{
    public static dynamic GetSecrets(IConfiguration configuration)
    {
        var vaultClient = GetVaultClient(configuration);
        var response = vaultClient.Secrets.KvV2Read(configuration["ServiceName"], StorageName);

        return response.Data.Data!;
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
