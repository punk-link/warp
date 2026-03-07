using Warp.WebApp.Helpers.Configuration.ConfigurationProviders;

namespace Warp.WebApp.Helpers.Configuration;

public static class ConsulHelper
{
    public static IConfigurationBuilder AddConsulConfiguration(this WebApplicationBuilder builder, string address, string token, IWebHostEnvironment environment, ILogger logger)
    {
        var storageName = BuildServiceName(builder.Configuration);
        return builder.Configuration.AddConsulConfiguration(address, token, storageName, environment, logger);
    }


    private static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder builder, string address, string token, string storageName, IWebHostEnvironment environment, ILogger logger)
        => builder.Add(new ConsulConfigurationSource(address, token, storageName, environment, logger));


    private static string BuildServiceName(IConfiguration configuration)
        => $"{configuration["ASPNETCORE_ENVIRONMENT"]}/{configuration["ServiceName"]}".ToLower();
}