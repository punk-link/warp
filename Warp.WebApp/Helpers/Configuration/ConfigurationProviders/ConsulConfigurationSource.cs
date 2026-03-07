namespace Warp.WebApp.Helpers.Configuration.ConfigurationProviders;

public class ConsulConfigurationSource : IConfigurationSource
{
    public ConsulConfigurationSource(string address, string token, string storageName, IWebHostEnvironment environment, ILogger logger)
    {
        _address = address;
        _environment = environment;
        _logger = logger;
        _storageName = storageName;
        _token = token;
    }


    public IConfigurationProvider Build(IConfigurationBuilder _)
        => new ConsulConfigurationProvider(_address, _token, _storageName, _environment, _logger);


    private readonly string _address;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger _logger;
    private readonly string _storageName;
    private readonly string _token;
}