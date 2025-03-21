using Microsoft.Extensions.Localization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Warp.WebApp.Models.Entries;

namespace Warp.WebApp.Services.OpenGraph;

public partial class OpenGraphService : IOpenGraphService
{
    public OpenGraphService(IStringLocalizer<ServerResources> localizer)
    {
        _localizer = localizer;
    }


    /// <summary>
    /// Builds an OpenGraph description from the provided description source and preview image URL.
    /// </summary>
    /// <param name="descriptionSource"></param>
    /// <param name="previewImageUrl"></param>
    /// <returns></returns>
    public EntryOpenGraphDescription BuildDescription(string descriptionSource, Uri? previewImageUrl)
    {
        var description = GetDescription(descriptionSource);
        return new EntryOpenGraphDescription(Title, description, GetImageUrl(previewImageUrl));
    }


    /// <summary>
    /// Returns the default OpenGraph description for the application.
    /// </summary>
    /// <returns></returns>
    public EntryOpenGraphDescription GetDefaultDescription() 
        => new(Title, _localizer["DefaultOpenGraphDescriptionText"], _defaultImageUrl);


    private string GetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return _localizer["DefaultOpenGraphDescriptionText"];

        var processedText = ProcessText(description);
        if (string.IsNullOrWhiteSpace(processedText))
            return _localizer["DefaultOpenGraphDescriptionText"];

        return TrimDescription(processedText);
    }


    private static string ProcessText(string text)
    {
        var textWithoutTags = HtmlTagsExpression().Replace(text, " ");
        var decodedText = WebUtility.HtmlDecode(textWithoutTags);
        
        return NormalizeWhitespace(decodedText);
    }


    private static string NormalizeWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        
        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            if (!char.IsControl(c) || c == ' ')
                sb.Append(c);
        }
        
        var normalizedText = ExtraSpaces().Replace(sb.ToString().Trim(), " ");
        return normalizedText.Trim();
    }


    /// <summary>
    /// Trims the description to a maximum length of 200 characters. 
    /// If the text is longer, it tries to end at a sentence boundary or a word boundary.
    /// Also triest to avoid cutting off words.
    /// </summary>
    private static string TrimDescription(string text)
    {
        const int maxDescriptionLength = 200;
        
        if (text.Length <= maxDescriptionLength)
            return text;

        var trimmed = text[..maxDescriptionLength].TrimEnd();
        
        var lastSentenceEnd = trimmed.LastIndexOf('.');
        if (lastSentenceEnd != -1 && lastSentenceEnd > maxDescriptionLength * 0.7)
            return trimmed[..(lastSentenceEnd + 1)];
        
        var lastSpace = trimmed.LastIndexOf(' ');
        if (lastSpace == -1)
            return trimmed + "...";
            
        var withoutLastWord = trimmed[..lastSpace];
        
        if (trimmed.Length - withoutLastWord.Length < 10 && withoutLastWord.Length > 50)
        {
            var prevSpace = withoutLastWord.LastIndexOf(' ');
            if (prevSpace > maxDescriptionLength * 0.7)
                return withoutLastWord[..prevSpace] + "...";
        }
        
        return withoutLastWord + "...";
    }


    private static Uri GetImageUrl(Uri? url)
    {
        if (url is null)
            return _defaultImageUrl;

        if (!url.IsAbsoluteUri)
            return _defaultImageUrl;
            
        return url;
    }


    private static readonly Uri _defaultImageUrl = new ("https://warp.punk.link/favicon.ico");
    private const string Title = "Warp";


    [GeneratedRegex(@"<(?:[^""'<>]|""[^""]*""|'[^']*')*?>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagsExpression();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex ExtraSpaces();


    private readonly IStringLocalizer<ServerResources> _localizer;
}