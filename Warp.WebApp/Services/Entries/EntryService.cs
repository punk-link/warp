using CSharpFunctionalExtensions;
using Warp.WebApp.Attributes;
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Validators;

namespace Warp.WebApp.Services.Entries;

/// <summary>
/// Implements functionality for managing entry content in the application.
/// </summary>
public sealed class EntryService : IEntryService
{
    /// <inheritdoc cref="IEntryService.Add"/>
    [TraceMethod]
    public async Task<Result<Entry, DomainError>> Add(EntryRequest entryRequest, CancellationToken cancellationToken)
    {
        return await BuildEntry()
            .Bind(Validate);


        Result<Entry, DomainError> BuildEntry()
        {
            var formattedText = TextFormatter.Format(entryRequest.TextContent);
            return new Entry(formattedText);
        }


        async Task<Result<Entry, DomainError>> Validate(Entry entry)
        {
            var validator = new EntryValidator(entryRequest);
            var validationResult = await validator.ValidateAsync(entry, cancellationToken);
            if (validationResult.IsValid)
                return entry;

            var error = new DomainError(LogEvents.EntryModelValidationError);
            foreach (var validationError in validationResult.Errors)
                error.WithExtension($"{nameof(EntryValidator)}:{validationError.ErrorCode}", validationError.ErrorMessage);

            return error;
        }
    }
}