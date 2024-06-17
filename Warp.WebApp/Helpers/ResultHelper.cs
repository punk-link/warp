using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Warp.WebApp.Models;

namespace Warp.WebApp.Helpers;

public static class ResultHelper
{
    public static Result<DummyObject, ProblemDetails> SuccessDummy()
        => Result.Success<DummyObject, ProblemDetails>(DummyObject.Empty);


    public static Result<T, ProblemDetails> NotFound<T>(IStringLocalizer<ServerResources> localizer)
        => Result.Failure<T, ProblemDetails>(ProblemDetailsHelper.CreateNotFound(localizer));
}