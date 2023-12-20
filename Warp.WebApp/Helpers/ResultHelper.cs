using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Warp.WebApp.Models;

namespace Warp.WebApp.Helpers;

public static class ResultHelper
{
    public static Result<DummyObject, ProblemDetails> SuccessDummy()
        => Result.Success<DummyObject, ProblemDetails>(DummyObject.Empty);
}