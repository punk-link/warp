using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Helpers;

namespace Warp.WebApp.Extensions;

public static class ResultExtensions
{
    public static Result<T, ProblemDetails> ToFailure<T>(this Result result)
        => Result.Failure<T, ProblemDetails>(ProblemDetailsHelper.Create(result.Error));
}