using FluentValidation;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Models.Validators;

public class EntryInfoValidator : AbstractValidator<EntryInfo>
{
    public EntryInfoValidator()
    {
        var error = DomainErrors.WarpExpirationPeriodEmpty();
        RuleFor(x => x.ExpiresAt).GreaterThan(default(DateTime))
            .WithErrorCode(error.Code.ToHttpStatusCodeInt().ToString())
            .WithMessage(error.Detail);
    }
}
