using System.Text.Json;
using System.Text.Json.Serialization;
using Warp.CodeGen.Models;

namespace Warp.CodeGen.Utilities;

public class LoggingConfigUtilities
{
    public static LoggingConfig? LoadFromFile(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath)) 
        {
            Console.Error.WriteLine($"Error: JSON file not found at {jsonFilePath}");
            return null;
        }
        
        string jsonContent = File.ReadAllText(jsonFilePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false) }
        };
        
        var loggingConfig = JsonSerializer.Deserialize<LoggingConfig>(jsonContent, options);
        if (loggingConfig?.LoggingCategories is null)
        {
            Console.Error.WriteLine("Invalid logging configuration format. No categories found.");
            return null;
        }
        
        return loggingConfig;
    }
}
