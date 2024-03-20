using Warp.WebApp.Pages.Shared.Components;

namespace Warp.WebApp.Services;

public static class OpenGraphService
{
    public static OpenGraphModel GetDefaultModel()
        => GetModel(DefaultDescription);


    public static OpenGraphModel GetModel(string description, List<string>? urls = null)
    {
        var processedDescription = GetDescription(description);
        var processedUrl = GetImageUrl(urls);

        return new OpenGraphModel(Title, processedDescription, processedUrl);
    }


    private static string GetDescription(string description)
    {
        const int maxDescriptionLength = 200;

        if (string.IsNullOrWhiteSpace(description))
            return DefaultDescription;

        var trimmedDescription = description.Trim();
        if (trimmedDescription.Length <= maxDescriptionLength)
            return trimmedDescription;

        var doubleTrimmedDescription = trimmedDescription[..maxDescriptionLength]
            .TrimEnd();

        var dotIndex = doubleTrimmedDescription.LastIndexOf('.');
        if (dotIndex == -1)
            return doubleTrimmedDescription[..(maxDescriptionLength - 3)] + "...";

        return doubleTrimmedDescription[..(dotIndex + 1)];
    }


    private static string GetImageUrl(List<string>? urls)
    {
        if (urls is null || urls.Count == 0)
            return DefaultImageUrl;

        return urls.First();
    }


    private const string DefaultDescription = "Warp is a simple and secure way to share text and images.";
    private const string DefaultImageUrl = "https://warp.punk.link/favicon.ico";
    private const string Title = "Warp";
}
