using Consul;
using System.Text;
using System.Text.Json.Nodes;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Helpers.Configuration.ConfigurationProviders;

public class ConsulConfigurationProvider : ConfigurationProvider
{
    public ConsulConfigurationProvider(string address, string token, string storageName, ILogger logger)
    {
        _address = $"http://{address}";
        _logger = logger;
        _storageName = storageName;
        _token = token;
    }


    public override void Load()
    {
        var consulClient = GetConsulClient(_address, _token);
        var result = consulClient.KV.Get(_storageName).GetAwaiter().GetResult();

        if (result.Response is null)
        {
            _logger.LogConsulKvKeyNotFound(_storageName);
            throw new InvalidOperationException($"Consul KV key '{_storageName}' was not found. Verify the key exists and the service name and environment are configured correctly.");
        }

        var configurationString = Encoding.UTF8.GetString(result.Response.Value, 0, result.Response.Value.Length);
        var jsonNode = JsonNode.Parse(configurationString);

        if (jsonNode is null)
        {
            _logger.LogConsulKvInvalidJson(_storageName);
            throw new InvalidOperationException($"Consul KV key '{_storageName}' returned null or invalid JSON.");
        }

        foreach (var (nodeKey, value) in jsonNode.AsObject().AsEnumerable())
            FlattenNode(value, nodeKey);
    }


    private void FlattenNode(JsonNode? node, string parentKey)
    {
        switch (node)
        {
            case JsonValue:
                Data.Add(parentKey, node.ToString());
                break;
            case JsonObject:
                foreach (var prop in node.AsObject().AsEnumerable())
                {
                    if (prop.Value is null)
                        continue;

                    FlattenNode(prop.Value, $"{parentKey}:{prop.Key}");
                }

                break;
            case JsonArray jsonArray:
                for (var i = 0; i < jsonArray.Count; i++)
                    FlattenNode(jsonArray[i], $"{parentKey}:{i}");

                break;
        }
    }


    private static ConsulClient GetConsulClient(string address, string token)
        => new(options =>
        {
            options.Address = new Uri(address);
            options.Token = token;
        });


    private readonly string _address;
    private readonly ILogger _logger;
    private readonly string _storageName;
    private readonly string _token;
}