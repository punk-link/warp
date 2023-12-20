using FluentValidation;

namespace Warp.WebApp.Models.Validators;

public class WarpContentValidator : AbstractValidator<WarpContent>
{
    public WarpContentValidator()
    {
        RuleFor(x => x.Content).NotEmpty()
            .WithErrorCode("100001")
            .WithMessage("Can't add the content. The body is empty.");
        
        RuleFor(x => x.ExpiresIn).GreaterThan(default(TimeSpan))
            .WithErrorCode("10002")
            .WithMessage("Can't add the content. The body is empty.");
    }
}