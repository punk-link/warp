using FluentValidation;
using Warp.WebApp.Constants.Logging;
using Warp.WebApp.Extensions;

namespace Warp.WebApp.Models.Validators;

public class EntryInfoValidator : AbstractValidator<EntryInfo>
{
    public EntryInfoValidator()
    {
        RuleFor(x => x.ExpiresAt).GreaterThan(default(DateTime))
            .WithErrorCode(((int)LogEvents.WarpExpirationPeriodEmpty).ToString())
            .WithMessage(LogEvents.WarpExpirationPeriodEmpty.ToDescriptionString());
    }
}
