using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using Warp.WebApp.Attributes;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Models.Validators;

namespace Warp.WebApp.Services.Entries;

/// <summary>
/// Implements functionality for managing entry content in the application.
/// </summary>
public sealed class EntryService : IEntryService
{
    public EntryService(IOptions<EntryValidatorOptions> entryValidatorOptions)
    {
        _validatorOptions = entryValidatorOptions.Value;
    }


    /// <inheritdoc cref="IEntryService.Add"/>
    [TraceMethod]
    public async Task<Result<Entry, DomainError>> Add(EntryRequest entryRequest, CancellationToken cancellationToken)
    {
        return await BuildEntry()
            .Bind(Validate);


        Result<Entry, DomainError> BuildEntry()
        {
            if (entryRequest.EditMode is EditMode.Advanced)
            {
                var sanitizedHtml = HtmlSanitizer.Sanitize(entryRequest.TextContent);
                return new Entry(content: sanitizedHtml, contentDelta: entryRequest.ContentDelta);
            }

            var normalizedText = PlainTextSanitizer.Sanitize(entryRequest.TextContent);
            return new Entry(content: normalizedText, contentDelta: null);
        }


        async Task<Result<Entry, DomainError>> Validate(Entry entry)
        {
            var validator = new EntryValidator(entryRequest, _validatorOptions);
            var validationResult = await validator.ValidateAsync(entry, cancellationToken);
            if (validationResult.IsValid)
                return entry;

            var error = DomainErrors.EntryModelValidationError();
            foreach (var validationError in validationResult.Errors)
                error.WithExtension($"{nameof(EntryValidator)}:{validationError.ErrorCode}", validationError.ErrorMessage);

            return error;
        }
    }


    private readonly EntryValidatorOptions _validatorOptions;
}