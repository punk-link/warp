using System.Text.RegularExpressions;

namespace Warp.WebApp.Services;

public static class TextFormatter
{
    public static string Format(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var paragraphs = text.ReplaceLineEndings()
            .Split(Environment.NewLine);

        var result = string.Empty;
        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph))
                continue;

            result += $"<p>{paragraph}</p>";
        }

        return result;
    }


    public static string GetCleanString(string htmlText)
    {
        if (string.IsNullOrWhiteSpace(htmlText))
            return string.Empty;

        const string pattern = @"<(.|\n)*?>";
        return Regex.Replace(htmlText, pattern, string.Empty, RegexOptions.Compiled);
    }
}