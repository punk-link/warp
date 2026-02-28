using Warp.WebApp.Helpers.Configuration.ConfigurationProviders;

namespace Warp.WebApp.Helpers.Configuration;

public static class ConsulHelper
{
    public static IConfigurationBuilder AddConsulConfiguration(this WebApplicationBuilder builder, string address, string token, ILogger logger)
    {
        var storageName = BuildServiceName(builder.Configuration);
        return builder.Configuration.AddConsulConfiguration(address, token, storageName, logger);
    }


    private static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder builder, string address, string token, string storageName, ILogger logger)
        => builder.Add(new ConsulConfigurationSource(address, token, storageName, logger));


    private static string BuildServiceName(IConfiguration configuration)
        => $"{configuration["ASPNETCORE_ENVIRONMENT"]}/{configuration["ServiceName"]}".ToLower();
}