using FluentValidation;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Constants.Logging;

namespace Warp.WebApp.Models.Validators;

public class EntryInfoValidator : AbstractValidator<EntryInfo>
{
    public EntryInfoValidator(IStringLocalizer<ServerResources> localizer)
    {
        RuleFor(x => x.ExpiresAt).GreaterThan(default(DateTime))
            .WithErrorCode(((int)LoggingEvents.WarpExpirationPeriodEmpty).ToString())
            .WithMessage(localizer["EntryExpirationPeriodEmptyErrorMessage"]);
    }
}
