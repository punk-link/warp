namespace Warp.WebApp.Helpers.Configuration;

/// <summary>
/// Provides helper methods for safely logging configuration data.
/// </summary>
internal static class ConfigurationLogHelper
{
    /// <summary>
    /// Builds a redacted string representation of configuration key-value pairs.
    /// Values whose key's last segment matches a known sensitive pattern are replaced with <c>[REDACTED]</c>.
    /// </summary>
    /// <param name="data">The configuration dictionary to format.</param>
    /// <returns>A comma-separated string of <c>Key=Value</c> pairs with sensitive values redacted.</returns>
    internal static string BuildRedactedConfigData(IDictionary<string, string?> data)
        => string.Join(", ", data.Select(kv => $"{kv.Key}={RedactValue(kv.Key, kv.Value)}"));


    private static string RedactValue(string key, string? value)
    {
        var lastSegment = key.Contains(':')
            ? key[(key.LastIndexOf(':') + 1)..]
            : key;

        return _sensitiveKeyPatterns.Any(p => lastSegment.Contains(p, StringComparison.OrdinalIgnoreCase))
            ? "[REDACTED]"
            : value ?? string.Empty;
    }


    private static readonly string[] _sensitiveKeyPatterns =
    [
        "password",
        "secret",
        "token",
        "key",
        "connectionstring",
        "credential",
    ];
}
