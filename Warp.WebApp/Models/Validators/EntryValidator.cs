using FluentValidation;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Constants.Logging;

namespace Warp.WebApp.Models.Validators;

public class EntryValidator : AbstractValidator<Entry>
{
    public EntryValidator(IStringLocalizer<ServerResources> localizer)
    {
        RuleFor(x => x.Content).NotEmpty()
            .WithErrorCode(LoggingConstants.WarpContentEmpty.ToString())
            .WithMessage(localizer["EntryBodyEmptyErrorMessage"]);
        
        RuleFor(x => x.ExpiresAt).GreaterThan(default(DateTime))
            .WithErrorCode(LoggingConstants.WarpExpirationPeriodEmpty.ToString())
            .WithMessage(localizer["EntryExpirationPeriodEmptyErrorMessage"]);
    }
}