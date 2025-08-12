using CSharpFunctionalExtensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Warp.WebApp.Data;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Options;

namespace Warp.WebApp.Services.OpenGraph;

public partial class OpenGraphService : IOpenGraphService
{
    public OpenGraphService(IOptionsSnapshot<OpenGraphOptions> options, IStringLocalizer<ServerResources> localizer, IDataStorage dataStorage)
    {
        _dataStorage = dataStorage;
        _localizer = localizer;
        _options = options.Value;
    }


    /// <inheritdoc />
    public Task<UnitResult<DomainError>> Add(Guid entryId, EntryOpenGraphDescription openGraphDescription, TimeSpan expiresIn, CancellationToken cancellationToken) 
        => AddInternal(entryId, openGraphDescription, expiresIn, cancellationToken);


    /// <inheritdoc />
    public Task<UnitResult<DomainError>> Add(Guid entryId, string descriptionSource, Uri? previewImageUri, TimeSpan expiresIn, CancellationToken cancellationToken)
    { 
        var description = BuildDescription(descriptionSource, previewImageUri);
        return AddInternal(entryId, description, expiresIn, cancellationToken);
    }


    //// <inheritdoc />
    public EntryOpenGraphDescription Get() 
        => new(_options.Title, _localizer["Warplyn is a simple and secure way to share text and images."], _options.DefaultImageUrl);


    /// <inheritdoc />
    public async Task<EntryOpenGraphDescription> Get(Guid entryId, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeyBuilder.BuildEntryOpenGraphDescriptionCacheKey(entryId);
        
        var description = await _dataStorage.TryGet<EntryOpenGraphDescription?>(cacheKey, cancellationToken);
        if (description is not null)
            return description.Value;

        return Get();
    }


    private Task<UnitResult<DomainError>> AddInternal(Guid entryId, EntryOpenGraphDescription openGraphDescription, TimeSpan expiresIn, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeyBuilder.BuildEntryOpenGraphDescriptionCacheKey(entryId);
        return _dataStorage.Set(cacheKey, openGraphDescription, expiresIn, cancellationToken);
    }


    private EntryOpenGraphDescription BuildDescription(string descriptionSource, Uri? previewImageUrl)
    {
        var description = ProcessAndTrimDescription(descriptionSource);
        return new EntryOpenGraphDescription(_options.Title, description, ResolveImageUrlOrDefault(previewImageUrl, _options.DefaultImageUrl));
    }


    private static string NormalizeWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        
        var sb = new StringBuilder(text.Length);
        
        foreach (var c in text)
        {
            if (ShouldReplaceWithSpace(c))
                sb.Append(' ');
            else
                sb.Append(c);
        }
        
        var normalized = ExtraSpaces().Replace(sb.ToString().Trim(), " ");
        // Fix spaces before punctuation
        var result = SpacesBeforePunctuation().Replace(normalized, "$1");
        
        return result;


        static bool ShouldReplaceWithSpace(char c)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
    
            return category == UnicodeCategory.Control || 
                   category == UnicodeCategory.Format || 
                   category == UnicodeCategory.LineSeparator || 
                   category == UnicodeCategory.ParagraphSeparator ||
                   category == UnicodeCategory.SpaceSeparator ||
                   // Zero-width characters
                   c == '\u200B' || c == '\u200C' || c == '\u200D' || 
                   c == '\u2060' || c == '\uFEFF';
        }
    }


    private string ProcessAndTrimDescription(string description)
    {
        var processedText = StripHtmlAndNormalizeWhitespace(description);
        if (string.IsNullOrWhiteSpace(processedText))
            return _localizer["Warplyn is a simple and secure way to share text and images."];

        return TrimDescription(processedText);
    }


    private static string StripHtmlAndNormalizeWhitespace(string text)
    {
        var textWithoutTags = HtmlTagsExpression().Replace(text, " ");
        var decodedText = WebUtility.HtmlDecode(textWithoutTags);
        
        return NormalizeWhitespace(decodedText);
    }


    private static Uri ResolveImageUrlOrDefault(Uri? url, Uri defaultUrl)
    {
        if (url is null || !url.IsAbsoluteUri)
            return defaultUrl;

        return url;
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


    [GeneratedRegex(@"<(?:[^""'<>]|""[^""]*""|'[^']*')*?>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagsExpression();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex ExtraSpaces();
    
    [GeneratedRegex(@"\s+([.,;:!?)])", RegexOptions.Compiled)]
    private static partial Regex SpacesBeforePunctuation();


    private readonly IDataStorage _dataStorage;
    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly OpenGraphOptions _options;
}