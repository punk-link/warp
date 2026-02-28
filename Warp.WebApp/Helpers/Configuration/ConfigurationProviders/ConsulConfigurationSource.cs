namespace Warp.WebApp.Helpers.Configuration.ConfigurationProviders;

public class ConsulConfigurationSource : IConfigurationSource
{
    public ConsulConfigurationSource(string address, string token, string storageName, ILogger logger)
    {
        _address = address;
        _logger = logger;
        _storageName = storageName;
        _token = token;
    }


    public IConfigurationProvider Build(IConfigurationBuilder _)
        => new ConsulConfigurationProvider(_address, _token, _storageName, _logger);


    private readonly string _address;
    private readonly ILogger _logger;
    private readonly string _storageName;
    private readonly string _token;
}