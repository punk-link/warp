using CSharpFunctionalExtensions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Helpers;
using Warp.WebApp.Models;
using Warp.WebApp.Models.ProblemDetails;

namespace Warp.WebApp.Extensions;

public static class ValidationResultExtensions
{
    public static Result<DummyObject, ProblemDetails> ToFailure(this ValidationResult validationResult, IStringLocalizer<ServerResources> localizer)
        => validationResult.ToFailure<DummyObject>(localizer);


    public static Result<T, ProblemDetails> ToFailure<T>(this ValidationResult validationResult, IStringLocalizer<ServerResources> localizer)
    {
        var errors = new List<Error>(validationResult.Errors.Count);
        foreach (var validationError in validationResult.Errors)
            errors.Add(new Error(validationError.ErrorCode, validationError.ErrorMessage));

        var details = ProblemDetailsHelper.Create(localizer["ModelValidationErrorMessage"]);
        details.AddErrors(errors);

        return Result.Failure<T, ProblemDetails>(details);
    }
}