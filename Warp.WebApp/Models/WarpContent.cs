namespace Warp.WebApp.Models;

public class WarpContent
{
    public WarpContent(Guid id, string content, DateTime createdAt, TimeSpan expiresIn)
    {
        Id = id;
        Content = content;
        CreatedAt = createdAt;
        ExpiresIn = expiresIn;
    }


    public Guid Id { get; }
    public string Content { get; }
    public DateTime CreatedAt {get; }
    public TimeSpan ExpiresIn { get; }
}