using CSharpFunctionalExtensions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;
using Warp.WebApp.Models.ProblemDetails;

namespace Warp.WebApp.Helpers;

public static class ValidationResultHelper
{
    public static Result<DummyObject, ProblemDetails> ToFailure(this ValidationResult validationResult)
        => ToFailure<DummyObject>(validationResult);
    
    
    public static Result<T, ProblemDetails> ToFailure<T>(this ValidationResult validationResult)
    {
        var errors = new List<Error>(validationResult.Errors.Count);
        foreach (var validationError in validationResult.Errors)
            errors.Add(new Error(validationError.ErrorCode, validationError.ErrorMessage));
        
        var details = ProblemDetailsHelper.Create("Model validation error");
        details.AddErrors(errors);

        return Result.Failure<T, ProblemDetails>(details);
    }
}