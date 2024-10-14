using Microsoft.Extensions.Localization;
using System.Net;
using System.Text.RegularExpressions;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.Infrastructure;

namespace Warp.WebApp.Services;

public partial class OpenGraphService : IOpenGraphService
{
    public OpenGraphService(IStringLocalizer<ServerResources> localizer, IUrlService urlService)
    {
        _localizer = localizer;
        _urlService = urlService;
    }


    public EntryOpenGraphDescription BuildDescription(Entry entry)
    {
        var description = GetDescription(entry.Content);
        var imageUrls = entry.ImageIds.Select(x => _urlService.GetImageUrl(entry.Id, x)).ToList();
        var previewImageUrl = GetImageUrl(imageUrls);

        return new EntryOpenGraphDescription(Title, description, previewImageUrl);
    }


    public EntryOpenGraphDescription GetDefaultDescription()
    {
        var description = GetDescription(string.Empty);
        return new EntryOpenGraphDescription(Title, description, _defaultImageUrl);
    }


    private string GetDescription(string description)
    {
        const int maxDescriptionLength = 200;

        if (string.IsNullOrWhiteSpace(description))
            return _localizer["DefaultOpenGraphDescriptionText"];

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


    private static Uri GetImageUrl(List<Uri> urls)
    {
        if (urls.Count == 0)
            return _defaultImageUrl;

        // TODO: check if the url is not a relative path
        return urls.First();
    }


    private static readonly Uri _defaultImageUrl = new ("https://warp.punk.link/favicon.ico");
    private const string Title = "Warp";


    [GeneratedRegex("<.*?>")]
    private static partial Regex HtmlTagsExpression();


    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly IUrlService _urlService;
}