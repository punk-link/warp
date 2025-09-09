using Microsoft.FeatureManagement;
using Warp.WebApp.Helpers.Configuration;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Extensions;

internal static class WebApplicationBuilderExtensions
{
    internal static async Task<WebApplicationBuilder> AddConfiguration(this WebApplicationBuilder builder, ILogger<Program> logger)
    {
        if (builder.Environment.IsLocal())
        {
            logger.LogLocalConfigurationIsInUse();
            builder.Configuration.AddJsonFile($"appsettings.{builder.Configuration["ASPNETCORE_ENVIRONMENT"]}.json", optional: true, reloadOnChange: true);
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
}
