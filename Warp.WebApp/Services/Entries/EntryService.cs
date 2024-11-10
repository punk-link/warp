using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Attributes;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Validators;

namespace Warp.WebApp.Services.Entries;

public sealed class EntryService : IEntryService
{
    public EntryService(IStringLocalizer<ServerResources> localizer)
    {
        _localizer = localizer;
    }


    [TraceMethod]
    public async Task<Result<Entry, ProblemDetails>> Add(EntryRequest entryRequest, CancellationToken cancellationToken)
    {
        return await BuildEntry()
            .Bind(Validate);


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
    }


    private readonly IStringLocalizer<ServerResources> _localizer;
}