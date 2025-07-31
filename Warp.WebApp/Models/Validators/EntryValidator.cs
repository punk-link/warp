using FluentValidation;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Models.Errors;

namespace Warp.WebApp.Models.Validators;

public class EntryValidator : AbstractValidator<Entry>
{
    public EntryValidator(EntryRequest entryRequest)
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
                break;
            // Business rule: Entry content must not be empty if the edit mode is Advanced and there are no images attached.
            case EditMode.Advanced when entryRequest.ImageIds.Count == 0:
                RuleFor(x => x.Content).NotEmpty()
                    .WithErrorCode(error.Code.ToHttpStatusCodeInt().ToString())
                    .WithMessage(error.Detail);
                break;
        }
    }
}
