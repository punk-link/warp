using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Attributes;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Validators;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.Infrastructure;

namespace Warp.WebApp.Services.Entries;

public sealed class EntryService : IEntryService
{
    public EntryService(IStringLocalizer<ServerResources> localizer, 
        IImageService imageService, 
        IUrlService urlService)
    {
        _imageService = imageService;
        _localizer = localizer;
        _urlService = urlService;
    }


    [TraceMethod]
    public async Task<Result<Entry, ProblemDetails>> Add(Guid entryInfoId, EntryRequest entryRequest, CancellationToken cancellationToken)
    {
        return await BuildEntry()
            .Bind(Validate)
            .Bind(AttachImages);


        Result<Entry, ProblemDetails> BuildEntry()
        {
            var formattedText = TextFormatter.Format(entryRequest.TextContent);
            return new Entry(formattedText);
        }


        async Task<Result<Entry, ProblemDetails>> Validate(Entry entry)
        {
            var validator = new EntryValidator(_localizer, entryRequest);
            var validationResult = await validator.ValidateAsync(entry, cancellationToken);
            if (!validationResult.IsValid)
                return validationResult.ToFailure<Entry>(_localizer);

            return entry;
        }


        async Task<Result<Entry, ProblemDetails>> AttachImages(Entry entry)
        {
            var attachedImageIds = await _imageService.Attach(entryRequest.ImageIds, cancellationToken);
            var imageUrls = new List<Uri>(attachedImageIds.Count);
            foreach (var imageId in attachedImageIds)
            {
                var url = _urlService.GetImageUrl(entryInfoId, in imageId);
                imageUrls.Add(url);
            }

            return entry with { ImageIds = attachedImageIds };
        }
    }


    private readonly IImageService _imageService;
    private readonly IStringLocalizer<ServerResources> _localizer;
    private readonly IUrlService _urlService;
}