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
        sb.AppendLine("using System;");
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
                
                sb.AppendLine($"    [LoggerMessage((int)LogEvents.{logEvent.Name}, LogLevel.{logEvent.LogLevel}, \"{logEvent.Description}\")]");
                if (logEvent.Obsolete)
                    sb.AppendLine($"    [Obsolete(\"This log message is obsolete. Do not use it.\")]");

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
            var paramName = match.Groups[1].Value;
            var paramType = "string?";
            
            if (paramName.Contains(':'))
            {
                var parts = paramName.Split(':');
                paramName = parts[0].Trim();
                paramType = parts[1].Trim();
            }

            var variableName = StringExtensions.ToCamelCase(paramName);
            parameters.Add($"{paramType} {variableName}");
        }
        
        return parameters.Count > 0 
            ? ", " + string.Join(", ", parameters) 
            : string.Empty;
    }

    
    // Extract parameters from the message template (e.g., {RequestId}, {ErrorMessage}, {ImageId:Guid})
    [GeneratedRegex(@"\{([^{}]+)\}", RegexOptions.Compiled)]
    private static partial Regex LogParametersRegex();
}