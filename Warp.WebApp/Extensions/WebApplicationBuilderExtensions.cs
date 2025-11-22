using Microsoft.FeatureManagement;
using Warp.WebApp.Helpers.Configuration;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Extensions;

internal static class WebApplicationBuilderExtensions
{
    internal static async Task<WebApplicationBuilder> AddConfiguration(this WebApplicationBuilder builder, ILogger<Program> logger)
    {
        var environmentName = builder.Environment.EnvironmentName;

        if (builder.Environment.IsLocal())
        {
            logger.LogLocalConfigurationIsInUse();
            builder.Configuration.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
        }
        else if (builder.Environment.IsEndToEndTests())
        {
            logger.LogEndToEndTestsConfigurationIsInUse();
            builder.Configuration.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
            AddVaultTokenFromFile(builder, logger);
        }
        else
        {
            var secrets = await VaultHelper.GetSecrets<ProgramSecrets>(logger, builder.Configuration);
            builder.AddConsulConfiguration(secrets.ConsulAddress, secrets.ConsulToken);

            builder.Configuration.AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("S3Options:SecretAccessKey", secrets.S3SecretAccessKey)
            ]);
        }

        builder.Services.AddFeatureManagement();

        return builder;
    }


    private static void AddVaultTokenFromFile(WebApplicationBuilder builder, ILogger logger)
    {
        var tokenFile = builder.Configuration["PNKL_VAULT_TOKEN_FILE"];
        if (string.IsNullOrWhiteSpace(tokenFile))
            return;

        var rootedPath = Path.IsPathRooted(tokenFile)
            ? tokenFile
            : Path.Combine(builder.Environment.ContentRootPath, "..", tokenFile);

        if (!File.Exists(rootedPath))
        {
            logger.LogVaultTokenFileNotFound(tokenFile: rootedPath);
            return;
        }

        var fileToken = File.ReadAllText(rootedPath).Trim();
        if (string.IsNullOrWhiteSpace(fileToken))
        {
            logger.LogVaultTokenFileIsEmpty(tokenFile: rootedPath);
            return;
        }

        builder.Configuration.AddInMemoryCollection(
        [
            new KeyValuePair<string, string?>("PNKL_VAULT_TOKEN", fileToken)
        ]);
    }
}
