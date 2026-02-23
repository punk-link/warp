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

                var plainTextSizeError = DomainErrors.EntryPlainTextSizeExceeded();
                RuleFor(x => x.Content)
                    .Must((content) => BeWithinContentSizeLimit(content, entryValidatorOptions.MaxPlainTextSizeBytes))
                    .WithErrorCode(plainTextSizeError.Code.ToHttpStatusCodeInt().ToString())
                    .WithMessage(plainTextSizeError.Detail);
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
            var htmlSizeError = DomainErrors.EntryHtmlSizeExceeded();
            RuleFor(x => x.Content)
                .Must((content) => BeWithinContentSizeLimit(content, entryValidatorOptions.MaxHtmlSizeBytes))
                .WithErrorCode(htmlSizeError.Code.ToHttpStatusCodeInt().ToString())
                .WithMessage(htmlSizeError.Detail);

            var contentDeltaRequiredError = DomainErrors.EntryContentDeltaRequired();
            RuleFor(x => x.ContentDelta)
                .NotEmpty()
                .WithErrorCode(contentDeltaRequiredError.Code.ToHttpStatusCodeInt().ToString())
                .WithMessage(contentDeltaRequiredError.Detail);

            var contentDeltaInvalidJsonError = DomainErrors.EntryContentDeltaInvalidJson();
            RuleFor(x => x.ContentDelta)
                .Must(BeValidJson)
                .WithErrorCode(contentDeltaInvalidJsonError.Code.ToHttpStatusCodeInt().ToString())
                .WithMessage(contentDeltaInvalidJsonError.Detail)
                .When(x => !string.IsNullOrWhiteSpace(x.ContentDelta));

            var contentDeltaSizeError = DomainErrors.EntryContentDeltaSizeExceeded();
            RuleFor(x => x.ContentDelta)
                .Must((json) => BeWithinContentSizeLimit(json, entryValidatorOptions.MaxContentDeltaSizeBytes))
                .WithErrorCode(contentDeltaSizeError.Code.ToHttpStatusCodeInt().ToString())
                .WithMessage(contentDeltaSizeError.Detail)
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
