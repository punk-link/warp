namespace Warp.WebApp.Models.ProblemDetails;

public readonly record struct Error
{
    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }


    public string Code { get; } 
    public string Message { get; } 
}