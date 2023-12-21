using FluentValidation;
using Warp.WebApp.Constants.Logging;

namespace Warp.WebApp.Models.Validators;

public class WarpContentValidator : AbstractValidator<WarpContent>
{
    public WarpContentValidator()
    {
        RuleFor(x => x.Content).NotEmpty()
            .WithErrorCode(LoggingConstants.WarpContentEmpty.ToString())
            .WithMessage("Can't add the content. The body is empty.");
        
        RuleFor(x => x.ExpiresIn).GreaterThan(default(TimeSpan))
            .WithErrorCode(LoggingConstants.WarpExpirationPeriodEmpty.ToString())
            .WithMessage("Can't add the content. The expiration period is empty.");
    }
}