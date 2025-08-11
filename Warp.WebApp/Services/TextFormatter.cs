using System.Text.RegularExpressions;

namespace Warp.WebApp.Services;

public static partial class TextFormatter
{
    /// <summary>
    /// Converts plain text into HTML paragraphs. 
    /// Each line break in the input text is transformed into a separate &lt;p&gt; HTML tag.
    /// </summary>
    public static string ConvertToHtmlParagraphs(string text)
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


    /// <summary>
    /// Strips HTML tags from the provided text.
    /// </summary>
    public static string StripHtmlTags(string htmlText)
    {
        if (string.IsNullOrWhiteSpace(htmlText))
            return string.Empty;

        return HTMLTagRegex().Replace(htmlText, string.Empty);
    }


    [GeneratedRegex(@"<(.|\n)*?>", RegexOptions.Compiled)]
    private static partial Regex HTMLTagRegex();
}