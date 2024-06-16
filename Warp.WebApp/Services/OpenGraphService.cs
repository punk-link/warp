using System.Net;
using System.Text.RegularExpressions;
using Warp.WebApp.Pages.Shared.Components;

namespace Warp.WebApp.Services;

public static partial class OpenGraphService
{
    public static OpenGraphModel GetDefaultModel(string defaultDescription)
        => GetModel(defaultDescription);


    public static OpenGraphModel GetModel(string description, List<string>? urls = null)
    {
        var processedUrl = GetImageUrl(urls);

        return new OpenGraphModel(Title, description, processedUrl);
    }


    public static string GetDescription(string description)
    {
        const int maxDescriptionLength = 200;

        if (string.IsNullOrWhiteSpace(description))
            return description;

        var decodedDescription = WebUtility.HtmlDecode(description);
        var descriptionWithoutTags = HtmlTagsExpression().Replace(maxDescriptionLength <= decodedDescription.Length 
            ? decodedDescription[..maxDescriptionLength] 
            : decodedDescription, string.Empty);

        var trimmedDescription = descriptionWithoutTags.Trim();
        if (trimmedDescription.Length < maxDescriptionLength)
            return trimmedDescription;

        var doubleTrimmedDescription = trimmedDescription[..maxDescriptionLength]
            .TrimEnd();

        var dotIndex = doubleTrimmedDescription.LastIndexOf('.');
        if (dotIndex != -1)
            return doubleTrimmedDescription[..(dotIndex + 1)];

        var spaceIndex = doubleTrimmedDescription.LastIndexOf(' ');
        var descriptionWithoutLastWord = doubleTrimmedDescription[..spaceIndex];
        if (descriptionWithoutLastWord.Length < maxDescriptionLength - 3)
            return descriptionWithoutLastWord + "...";

        spaceIndex = descriptionWithoutLastWord.LastIndexOf(' ');
        return descriptionWithoutLastWord[..spaceIndex] + "...";
    }


    private static string GetImageUrl(List<string>? urls)
    {
        if (urls is null || urls.Count == 0)
            return DefaultImageUrl;

        return urls.First();
    }


    private const string DefaultImageUrl = "https://warp.punk.link/favicon.ico";
    private const string Title = "Warp";


    [GeneratedRegex("<.*?>")]
    private static partial Regex HtmlTagsExpression();
}
