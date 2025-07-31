using CSharpFunctionalExtensions;
using Warp.WebApp.Models.Errors;
using Warp.WebApp.Services;

namespace Warp.WebApp.Models.Entries.Converters;

public static class EntryInfoConverters
{
    public static async Task<Result<EntryApiResponse, DomainError>> ToEntryApiResponse(this Task<Result<EntryInfo, DomainError>> target)
    {
        var result = await target;
        return result.ToEntryApiResponse();
    }


    public static Result<EntryApiResponse, DomainError> ToEntryApiResponse(this Result<EntryInfo, DomainError> target)
    {
        if (target.IsFailure)
            return Result.Failure<EntryApiResponse, DomainError>(target.Error);

        return Result.Success<EntryApiResponse, DomainError>(target.Value.ToEntryApiResponse());
    }


    public static EntryApiResponse ToEntryApiResponse(this EntryInfo target) 
        => new (IdCoder.Encode(target.Id), target);
}
