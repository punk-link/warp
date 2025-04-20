using System.Text;
using Warp.CodeGen.Models;
using Warp.CodeGen.Utilities;

namespace Warp.CodeGen.Generators;

/// <summary>
/// Generates the LoggingConstants.cs file containing enum definitions
/// </summary>
public class ConstantsGenerator
{
    public static void Generate(LoggingConfig loggingConfig, string outputFilePath)
    {
        Console.WriteLine($"Generating logging constants to {outputFilePath}");

        var sb = new StringBuilder();
        sb.AppendLine("// This file is auto-generated. Do not edit directly.");
        sb.AppendLine("// Generated on: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
        sb.AppendLine("using System.ComponentModel;");
        sb.AppendLine();
        sb.AppendLine("namespace Warp.WebApp.Constants.Logging;");
        sb.AppendLine();
        sb.AppendLine("public enum LoggingEvents");
        sb.AppendLine("{");
        
        foreach (var category in loggingConfig.LoggingCategories)
        {
            var isLastCategory = category == loggingConfig.LoggingCategories.Last();

            sb.AppendLine($"    // {category.Name}");
            
            foreach (var logEvent in category.Events)
            {
                sb.AppendLine($"    [Description(\"{logEvent.Description}\")]");
                sb.AppendLine($"    {logEvent.Name} = {logEvent.Id},");
                if (logEvent != category.Events.Last() || !isLastCategory)
                    sb.AppendLine();
            }
            
            if (!isLastCategory)
                sb.AppendLine();
        }
        
        sb.AppendLine("}");
        
        FileUtilities.WriteToFile(outputFilePath, sb.ToString());
    }
}