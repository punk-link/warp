using FluentValidation;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Models.Entries.Enums;

namespace Warp.WebApp.Models.Validators;

public class EntryValidator : AbstractValidator<Entry>
{
    public EntryValidator(IStringLocalizer<ServerResources> localizer, EntryRequest entryRequest)
    {
        switch (entryRequest.EditMode)
        {
            case EditMode.Unset:
                RuleFor(x => x.Content).NotEmpty()
                    .WithErrorCode(LoggingConstants.WarpContentEmpty.ToString())
                    .WithMessage(localizer["EntryBodyEmptyErrorMessage"]);
                break;
            // Business rule: Entry content must not be empty if the edit mode is Text.
            case EditMode.Simple:
                RuleFor(x => x.Content).NotEmpty()
                    .WithErrorCode(LoggingConstants.WarpContentEmpty.ToString())
                    .WithMessage(localizer["EntryBodyEmptyErrorMessage"]);
                break;
            // Business rule: Entry content must not be empty if the edit mode is Advanced and there are no images attached.
            case EditMode.Advanced when entryRequest.ImageIds.Count == 0:
                RuleFor(x => x.Content).NotEmpty()
                    .WithErrorCode(LoggingConstants.WarpContentEmpty.ToString())
                    .WithMessage(localizer["EntryBodyEmptyErrorMessage"]);
                break;
        }
    }
}
