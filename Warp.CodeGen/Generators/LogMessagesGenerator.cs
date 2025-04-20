using System.Text;
using System.Text.RegularExpressions;
using Warp.CodeGen.Models;
using Warp.CodeGen.Utilities;

namespace Warp.CodeGen.Generators;

/// <summary>
/// Generates the LogMessages.cs file containing partial method declarations for structured logging
/// </summary>
public partial class LogMessagesGenerator
{
    public static void Generate(LoggingConfig loggingConfig, string outputFilePath)
    {
        Console.WriteLine($"Generating logging messages to {outputFilePath}");
        
        var sb = new StringBuilder();
        sb.AppendLine("// This file is auto-generated. Do not edit directly.");
        sb.AppendLine("// Generated on: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine("using Warp.WebApp.Constants.Logging;");
        sb.AppendLine();
        sb.AppendLine("namespace Warp.WebApp.Telemetry.Logging;");
        sb.AppendLine();
        sb.AppendLine("internal static partial class LogMessages");
        sb.AppendLine("{");
        
        foreach (var category in loggingConfig.LoggingCategories)
        {
            var isLastCategory = category == loggingConfig.LoggingCategories.Last(c => c.Events.Any(e => e.GenerateLogMessage));

            var categoryHasGeneratedEvents = category.Events.Any(e => e.GenerateLogMessage);
            if (!categoryHasGeneratedEvents)
                continue;
                
            sb.AppendLine($"    // {category.Name}");
            
            foreach (var logEvent in category.Events)
            {
                if (!logEvent.GenerateLogMessage)
                    continue;
                    
                var methodName = $"Log{logEvent.Name}";
                var parameters = ExtractParameters(logEvent.Description);
                
                sb.AppendLine($"    [LoggerMessage((int)LoggingEvents.{logEvent.Name}, LogLevel.{logEvent.LogLevel}, \"{logEvent.Description}\")]");
                sb.AppendLine($"    public static partial void {methodName}(this ILogger logger{parameters});");
                
                var isLastEventInCategory = logEvent == category.Events.Last(e => e.GenerateLogMessage);
                if (!isLastEventInCategory || !isLastCategory)
                    sb.AppendLine();
            }
            
            if (!isLastCategory)
                sb.AppendLine();
        }
        
        sb.AppendLine("}");
        
        FileUtilities.WriteToFile(outputFilePath, sb.ToString());
    }

    
    private static string ExtractParameters(string description)
    {
        var parameters = new List<string>();
        
        var paramMatches = LogParametersRegex().Matches(description);
        foreach (Match match in paramMatches)
        {
            string paramName = match.Groups[1].Value;
            
            // Special case handling for known parameter types
            if (paramName.Equals("RequestId", StringComparison.OrdinalIgnoreCase))
                parameters.Add("string? requestId");
            else if (paramName.Equals("ErrorMessage", StringComparison.OrdinalIgnoreCase))
                parameters.Add("string errorMessage");
            else if (paramName.Equals("ImageId", StringComparison.OrdinalIgnoreCase))
                parameters.Add("Guid imageId");
            else if (paramName.Equals("FileExtension", StringComparison.OrdinalIgnoreCase))
                parameters.Add("string fileExtension");
            else if (paramName.Equals("PartialViewName", StringComparison.OrdinalIgnoreCase))
                parameters.Add("string partialViewName");
            else if (paramName.Equals("Signature", StringComparison.OrdinalIgnoreCase))
                parameters.Add("string signature");
            else if (paramName.Equals("CacheValue", StringComparison.OrdinalIgnoreCase))
                parameters.Add("string? cacheValue");
            else
                parameters.Add($"string {StringExtensions.ToCamelCase(paramName)}");
        }
        
        return parameters.Count > 0 
            ? ", " + string.Join(", ", parameters) 
            : string.Empty;
    }

    
    // Extract parameters from the message template (e.g., {RequestId}, {ErrorMessage})
    [GeneratedRegex(@"\{([^{}]+)\}", RegexOptions.Compiled)]
    private static partial Regex LogParametersRegex();
}