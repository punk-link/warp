using System.Text;
using Warp.CodeGen.Models;
using Warp.CodeGen.Utilities;

namespace Warp.CodeGen.Generators;

/// <summary>
/// Generates the LoggingConstants.cs file containing enum definitions
/// </summary>
public class ConstantsGenerator : BaseGenerator
{
    public static void Generate(LoggingConfig loggingConfig, string outputFilePath)
    {
        Console.WriteLine($"Generating logging constants to {outputFilePath}");

        var sb = new StringBuilder();
        sb.AppendLine("// This file is auto-generated. Do not edit directly.");
        sb.AppendLine("// Generated on: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
        sb.AppendLine("using System;");
        sb.AppendLine("using System.ComponentModel;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using Warp.WebApp.Attributes;");
        
        sb.AppendLine();
        sb.AppendLine("namespace Warp.WebApp.Constants.Logging;");
        sb.AppendLine();
        sb.AppendLine("public enum LogEvents");
        sb.AppendLine("{");
        
        foreach (var category in loggingConfig.LoggingCategories)
        {
            var isLastCategory = category == loggingConfig.LoggingCategories.Last();

            sb.AppendLine($"    // {category.Name}");
            
            foreach (var logEvent in category.Events)
            {
                if (logEvent.Obsolete)
                    sb.AppendLine($"    [Obsolete(\"This logging event is obsolete and will be removed in a future version.\")]");
                
                var description = logEvent.DomainErrorDescription ?? logEvent.Description;
                sb.AppendLine($"    [Description(\"{description}\")]");
                sb.AppendLine($"    [HttpStatusCode({logEvent.HttpCode})]");
                sb.AppendLine($"    {logEvent.Name} = {logEvent.Id},");
                
                var isLastEventInCategory = logEvent == category.Events.Last();
                if (isLastEventInCategory && !isLastCategory)
                    sb.AppendLine();
            }
            
            if (!isLastCategory)
                sb.AppendLine();
        }
        
        sb.AppendLine("}");
        
        FileUtilities.WriteToFile(outputFilePath, sb.ToString());
    }
}