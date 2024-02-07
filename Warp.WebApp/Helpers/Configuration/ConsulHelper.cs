using Warp.WebApp.Helpers.Configuration.ConfigurationProviders;

namespace Warp.WebApp.Helpers.Configuration;

public static class ConsulHelper
{
    public static IConfigurationBuilder AddConsulConfiguration(this WebApplicationBuilder builder, string address, string token)
    {
        var storageName = BuildServiceName(builder.Configuration);
        return builder.Configuration.AddConsulConfiguration(address, token, storageName);
    }


    private static IConfigurationBuilder AddConsulConfiguration(this IConfigurationBuilder builder, string address, string token, string storageName) 
        => builder.Add(new ConsulConfigurationSource(address, token, storageName));


    private static string BuildServiceName(IConfiguration configuration)
        => $"{configuration["ASPNETCORE_ENVIRONMENT"]}/{configuration["ServiceName"]}".ToLower();
}