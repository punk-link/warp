using FluentValidation;
using System.Text.Json;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Models.Options;

namespace Warp.WebApp.Models.Validators;

public class EntryValidator : AbstractValidator<Entry>
{
    public EntryValidator(EntryRequest entryRequest, EntryValidatorOptions entryValidatorOptions)
    {
        var error = DomainErrors.WarpContentEmpty();
        switch (entryRequest.EditMode)
        {
            case EditMode.Unset:
                RuleFor(x => x.Content).NotEmpty()
                    .WithErrorCode(error.Code.ToHttpStatusCodeInt().ToString())
                    .WithMessage(error.Detail);
                break;
            // Business rule: Entry content must not be empty if the edit mode is Text.
            case EditMode.Simple:
                RuleFor(x => x.Content).NotEmpty()
                    .WithErrorCode(error.Code.ToHttpStatusCodeInt().ToString())
                    .WithMessage(error.Detail);

                RuleFor(x => x.Content)
                    .Must((content) => BeWithinContentSizeLimit(content, entryValidatorOptions.MaxContentDeltaSizeBytes))
                    .WithMessage($"Content size must not exceed {entryValidatorOptions.MaxContentDeltaSizeBytes / 1024} KB");
                break;
            // Business rule: Entry content must not be empty if the edit mode is Advanced and there are no images attached.
            case EditMode.Advanced when entryRequest.ImageIds.Count == 0:
                RuleFor(x => x.Content)
                    .Must(HaveNonEmptyContent)
                    .WithErrorCode(error.Code.ToHttpStatusCodeInt().ToString())
                    .WithMessage(error.Detail);
                break;
        }

        // Business rule: Content size limits and ContentDelta validation for Advanced mode
        if (entryRequest.EditMode == EditMode.Advanced)
        {
            RuleFor(x => x.Content)
                .Must((content) => BeWithinContentSizeLimit(content, entryValidatorOptions.MaxHtmlSizeBytes))
                .WithMessage($"HTML content size must not exceed {entryValidatorOptions.MaxHtmlSizeBytes / 1024} KB");

            RuleFor(x => x.ContentDelta)
                .NotEmpty()
                .WithMessage("ContentDelta is required for Advanced mode entries");

            RuleFor(x => x.ContentDelta)
                .Must(BeValidJson)
                .WithMessage("ContentDelta must be valid JSON")
                .When(x => !string.IsNullOrWhiteSpace(x.ContentDelta));

            RuleFor(x => x.ContentDelta)
                .Must((json) => BeWithinContentSizeLimit(json, entryValidatorOptions.MaxContentDeltaSizeBytes))
                .WithMessage($"ContentDelta size must not exceed {entryValidatorOptions.MaxContentDeltaSizeBytes / 1024} KB")
                .When(x => !string.IsNullOrWhiteSpace(x.ContentDelta));
        }
    }


    private static bool BeValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using var document = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }


    private static bool BeWithinContentSizeLimit(string? content, int maxSizeBytes)
    {
        if (string.IsNullOrWhiteSpace(content))
            return true;

        var sizeInBytes = System.Text.Encoding.UTF8.GetByteCount(content);
        return sizeInBytes <= maxSizeBytes;
    }


    private static bool HaveNonEmptyContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;

        var plainText = Services.HtmlSanitizer.GetPlainText(content);
        return !string.IsNullOrWhiteSpace(plainText);
    }
}
