using FluentValidation;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Models.Entries.Enums;

namespace Warp.WebApp.Models.Validators;

public class EntryValidator : AbstractValidator<Entry>
{
    public EntryValidator(IStringLocalizer<ServerResources> localizer, List<Guid> imageIds)
    {
        // Business rule: Entry content must not be empty if the edit mode is Advanced and there are no images attached.
        When(x => x.EditMode == EditMode.Advanced, () =>
        {
            When(_ => imageIds.Count == 0, () =>
            {
                RuleFor(x => x.Content).NotEmpty()
                    .WithErrorCode(LoggingConstants.WarpContentEmpty.ToString())
                    .WithMessage(localizer["EntryBodyEmptyErrorMessage"]);
            });
        });

        // Business rule: Entry content must not be empty if the edit mode is Text.
        When(x => x.EditMode == EditMode.Text, () =>
        {
            RuleFor(x => x.Content).NotEmpty()
                .WithErrorCode(LoggingConstants.WarpContentEmpty.ToString())
                .WithMessage(localizer["EntryBodyEmptyErrorMessage"]);
        });

        RuleFor(x => x.ExpiresAt).GreaterThan(default(DateTime))
            .WithErrorCode(LoggingConstants.WarpExpirationPeriodEmpty.ToString())
            .WithMessage(localizer["EntryExpirationPeriodEmptyErrorMessage"]);
    }
}