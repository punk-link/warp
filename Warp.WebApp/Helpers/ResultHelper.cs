using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Warp.WebApp.Models;

namespace Warp.WebApp.Helpers;

public static class ResultHelper
{
    public static Result<DummyObject, ProblemDetails> SuccessDummy()
        => Result.Success<DummyObject, ProblemDetails>(DummyObject.Empty);


    public static Result<T, ProblemDetails> NotFound<T>(string? detail = "Content not found.")
        => Result.Failure<T, ProblemDetails>(ProblemDetailsHelper.Create(detail!, HttpStatusCode.NotFound));
}