namespace Warp.WebApp.Helpers.Configuration;

/// <summary>
/// Provides normalization helpers for environment variable values.
/// </summary>
public static class EnvironmentVariableHelper
{
    /// <summary>
    /// Trims leading and trailing whitespace from an environment variable value.
    /// </summary>
    /// <param name="value">The raw environment variable value.</param>
    /// <returns>The trimmed value, or <see cref="string.Empty"/> when the input is null or whitespace.</returns>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Trim();
    }


    /// <summary>
    /// Trims leading and trailing whitespace and ensures a single trailing slash for URL-like environment variable values.
    /// </summary>
    /// <param name="value">The raw URL environment variable value.</param>
    /// <returns>The normalized URL value, or <see cref="string.Empty"/> when the input is null or whitespace.</returns>
    public static string NormalizeUrl(string? value)
    {
        var normalizedValue = Normalize(value);
        if (string.IsNullOrEmpty(normalizedValue))
            return string.Empty;

        return normalizedValue.TrimEnd('/') + "/";
    }
}