namespace Warp.WebApp.Models;

public class WarpContent
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public required TimeSpan ExpiresIn { get; set; }
}