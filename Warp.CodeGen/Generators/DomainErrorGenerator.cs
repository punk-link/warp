using System.Text;
using Warp.CodeGen.Models;
using Warp.CodeGen.Utilities;

namespace Warp.CodeGen.Generators;

/// <summary>
/// Generates the DomainErrors.cs file containing static methods for creating DomainError instances
/// </summary>
public partial class DomainErrorGenerator : BaseGenerator
{
    public static void Generate(LoggingConfig loggingConfig, string outputFilePath)
    {
        Console.WriteLine($"Generating domain errors to {outputFilePath}");
        
        var sb = new StringBuilder();
        sb.AppendLine("// This file is auto-generated. Do not edit directly.");
        sb.AppendLine("// Generated on: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
        sb.AppendLine("using Warp.WebApp.Constants.Logging;");
        sb.AppendLine("using Warp.WebApp.Models.Errors;");
        sb.AppendLine();
        sb.AppendLine("namespace Warp.WebApp.Errors;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Provides static methods to create domain error instances");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class DomainErrors");
        sb.AppendLine("{");
        
        foreach (var category in loggingConfig.LoggingCategories)
        {
            var isLastCategory = category == loggingConfig.LoggingCategories.Last();
            
            if (!string.IsNullOrEmpty(category.Name))
                sb.AppendLine($"    // {category.Name}");
            
            foreach (var logEvent in category.Events)
            {
                if (logEvent.Obsolete)
                    sb.AppendLine($"    [Obsolete(\"This error is obsolete. Do not use.\")]");

                var methodParameters = ExtractParameters(logEvent.DomainErrorDescription);
                var methodArguments = GetMethodArguments(logEvent.DomainErrorDescription);
                
                sb.AppendLine($"    public static DomainError {logEvent.Name}({methodParameters})");
                
                if (!string.IsNullOrEmpty(logEvent.DomainErrorDescription) && !string.IsNullOrEmpty(methodArguments))
                    sb.AppendLine($"        => new(LogEvents.{logEvent.Name}, string.Format(\"{logEvent.DomainErrorDescription}\", {methodArguments}));");
                else 
                    sb.AppendLine($"        => new(LogEvents.{logEvent.Name});");
                
                var isLastEventInCategory = logEvent == category.Events.Last();
                if (!isLastEventInCategory || !isLastCategory)
                    sb.AppendLine();
            }
            
            if (!isLastCategory && !string.IsNullOrEmpty(category.Name))
                sb.AppendLine();
        }
        
        sb.AppendLine("}");
        
        FileUtilities.WriteToFile(outputFilePath, sb.ToString());
    }
}