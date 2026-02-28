using Consul;
using System.Text;
using System.Text.Json.Nodes;

namespace Warp.WebApp.Helpers.Configuration.ConfigurationProviders;

public class ConsulConfigurationProvider : ConfigurationProvider
{
    public ConsulConfigurationProvider(string address, string token, string storageName)
    {
        _address = $"http://{address}";
        _storageName = storageName;
        _token = token;
    }


    public override void Load()
    {
        var consulClient = GetConsulClient(_address, _token);
        var configuration = consulClient.KV.Get(_storageName).GetAwaiter().GetResult();
        var configurationString = Encoding.UTF8.GetString(configuration.Response.Value, 0, configuration.Response.Value.Length);
        var jsonNode = JsonNode.Parse(configurationString);

        if (jsonNode is null)
            return;

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
                for (var i = 0; i < jsonArray.Count; i++ )
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
    private readonly string _storageName;
    private readonly string _token;
}