using System.Text.RegularExpressions;
using Warp.CodeGen.Utilities;

namespace Warp.CodeGen.Generators;

/// <summary>
/// Base class with common functionality for code generators
/// </summary>
public abstract partial class BaseGenerator
{
    /// <summary>
    /// Extracts parameter declarations from a message template.
    /// </summary>
    /// <param name="description">The message template containing parameters like {ParameterName} or {ParameterName:Type}</param>
    /// <returns>A string containing the parameter declarations</returns>
    protected static string ExtractParameters(string? description)
    {
        if (string.IsNullOrEmpty(description))
            return string.Empty;
            
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
            ? string.Join(", ", parameters) 
            : string.Empty;
    }


    /// <summary>
    /// Extracts method arguments from a message template.
    /// </summary>
    /// <param name="description">The message template containing parameters</param>
    /// <returns>A string containing the method arguments</returns>
    protected static string GetMethodArguments(string? description)
    {
        if (string.IsNullOrEmpty(description))
            return string.Empty;
            
        var arguments = new List<string>();
        var paramMatches = LogParametersRegex().Matches(description);
        
        foreach (Match match in paramMatches)
        {
            var paramName = match.Groups[1].Value;
            if (paramName.Contains(':'))
            {
                var parts = paramName.Split(':');
                paramName = parts[0].Trim();
            }
            
            arguments.Add(StringExtensions.ToCamelCase(paramName));
        }
        
        return string.Join(", ", arguments);
    }


    /// <summary>
    /// Strips type annotations from message template placeholders so the string is safe
    /// to use as a <c>[LoggerMessage]</c> template.
    /// For example, <c>{ImageId:Guid}</c> becomes <c>{ImageId}</c>.
    /// </summary>
    protected static string StripTypeAnnotations(string? description)
    {
        if (string.IsNullOrEmpty(description))
            return string.Empty;

        return LogParametersRegex().Replace(description, match =>
        {
            var placeholder = match.Groups[1].Value;
            if (placeholder.Contains(':'))
                placeholder = placeholder.Split(':')[0].Trim();

            return $"{{{placeholder}}}";
        });
    }


    /// <summary>
    /// Converts named format placeholders like {Name:type} to indexed format placeholders {0}, {1}, etc.
    /// </summary>
    /// <param name="formatString">The format string with named placeholders</param>
    /// <returns>A format string with indexed placeholders</returns>
    protected static string ConvertToIndexedFormat(string? formatString)
    {
        if (string.IsNullOrEmpty(formatString))
            return string.Empty;

        var parameterIndices = new Dictionary<string, int>();
        var currentIndex = 0;

        return LogParametersRegex().Replace(formatString, match =>
        {
            var paramName = match.Groups[1].Value;
            if (paramName.Contains(':'))
            {
                var parts = paramName.Split(':');
                paramName = parts[0].Trim();
            }

            if (!parameterIndices.TryGetValue(paramName, out int index))
            {
                index = currentIndex++;
                parameterIndices[paramName] = index;
            }

            return "{" + index + "}";
        });
    }


    /// <summary>
    /// Regex to extract parameters from message templates like {RequestId}, {ErrorMessage}, {ImageId:Guid}
    /// </summary>
    [GeneratedRegex(@"\{([^{}]+)\}", RegexOptions.Compiled)]
    protected static partial Regex LogParametersRegex();
}