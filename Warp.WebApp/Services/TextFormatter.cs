using System.Text.RegularExpressions;

namespace Warp.WebApp.Services;

public static partial class TextFormatter
{
    /// <summary>
    /// Normalizes raw user text for storage while preserving Markdown semantics:
    /// - Normalizes all line endings to '\n'
    /// - Preserves blank lines (paragraph separators)
    /// - Trims trailing spaces and tabs on non-blank lines
    /// - Converts whitespace-only lines to empty string
    /// Does not strip HTML so that future Markdown (which can allow or post-process raw HTML) has the original source.
    /// </summary>
    public static string NormalizeForMarkdownSource(string text)
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