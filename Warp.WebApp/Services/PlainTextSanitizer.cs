using System.Text.RegularExpressions;

namespace Warp.WebApp.Services;

public static partial class PlainTextSanitizer
{
    /// <summary>
    /// Normalizes raw plain text for storage:
    /// - Normalizes all line endings to '\n'
    /// - Preserves blank lines (paragraph separators)
    /// - Trims trailing spaces and tabs on non-blank lines
    /// - Converts whitespace-only lines to empty string
    /// WARNING: Do NOT use for HTML content as it will strip formatting-significant whitespace and add newlines that mangle HTML.
    /// </summary>
    public static string Sanitize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var normalized = text.ReplaceLineEndings("\n");
        var lines = normalized.Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                lines[i] = string.Empty;
                continue;
            }

            lines[i] = TrailingSpaceRegex().Replace(line, string.Empty);
        }

        var result = string.Join('\n', lines);
        return result.TrimEnd('\n') + '\n';
    }


    [GeneratedRegex(@"[ \t]+$", RegexOptions.Compiled)]
    private static partial Regex TrailingSpaceRegex();
}