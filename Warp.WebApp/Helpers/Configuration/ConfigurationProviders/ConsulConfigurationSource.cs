namespace Warp.WebApp.Helpers.Configuration.ConfigurationProviders;

public class ConsulConfigurationSource : IConfigurationSource
{
    public ConsulConfigurationSource(string address, string token, string storageName)
    {
        _address = address;
        _storageName = storageName;
        _token = token;
    }


    public IConfigurationProvider Build(IConfigurationBuilder _)
    {
        return new ConsulConfigurationProvider(_address, _token, _storageName);
    }


    private readonly string _address;
    private readonly string _storageName;
    private readonly string _token;
}