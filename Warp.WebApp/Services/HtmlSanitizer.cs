namespace Warp.WebApp.Services;

/// <summary>
/// Static utility for sanitizing HTML content to prevent XSS attacks.
/// Configured to allow only Tiptap editor output tags and attributes.
/// </summary>
public static partial class HtmlSanitizer
{
    static HtmlSanitizer()
    {
        _sanitizer = new Ganss.Xss.HtmlSanitizer();
        ConfigureSanitizer();

        _textExtractor = new Ganss.Xss.HtmlSanitizer();
        ConfigureTextExtractor();
    }


    /// <summary>
    /// Extracts plain text from HTML by removing all tags and normalizing whitespace.
    /// Uses HtmlSanitizer for robust handling of malformed HTML and edge cases.
    /// </summary>
    /// <param name="html">The HTML content</param>
    /// <returns>Plain text content without HTML tags</returns>
    public static string GetPlainText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var plainText = _textExtractor.Sanitize(html);
        plainText = LongSpaceRegex().Replace(plainText, " ");
        return plainText.Trim();
    }


    /// <summary>
    /// Sanitizes the specified HTML content, removing potentially dangerous tags, attributes, and scripts
    /// </summary>
    /// <param name="html">The HTML content to sanitize</param>
    /// <returns>The sanitized HTML string with only allowed tags and attributes</returns>
    public static string Sanitize(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        return _sanitizer.Sanitize(html);
    }


    private static void ConfigureSanitizer()
    {
        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedSchemes.Clear();

        var allowedTags = new[]
        {
            "b", "i", "u", "s", "em", "strong",
            "h1", "h2", "h3",
            "p", "br",
            "ol", "ul", "li",
            "a",
            "blockquote",
            "pre", "code",
            "span"
        };

        foreach (var tag in allowedTags)
            _sanitizer.AllowedTags.Add(tag);

        _sanitizer.AllowedAttributes.Add("href");
        _sanitizer.AllowedAttributes.Add("target");
        _sanitizer.AllowedAttributes.Add("rel");

        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("mailto");

        _sanitizer.AllowedCssProperties.Clear();

        _sanitizer.AllowDataAttributes = false;
    }


    private static void ConfigureTextExtractor()
    {
        _textExtractor.AllowedTags.Clear();
        _textExtractor.AllowedAttributes.Clear();
        _textExtractor.AllowedSchemes.Clear();
        _textExtractor.AllowedCssProperties.Clear();
        _textExtractor.AllowDataAttributes = false;
        _textExtractor.KeepChildNodes = true;
    }


    [System.Text.RegularExpressions.GeneratedRegex(@"\s+")]
    private static partial System.Text.RegularExpressions.Regex LongSpaceRegex();


    private static readonly Ganss.Xss.HtmlSanitizer _sanitizer;
    private static readonly Ganss.Xss.HtmlSanitizer _textExtractor;
}
