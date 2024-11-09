namespace Warp.WebApp.Models.Files;

public readonly record struct FileContent
{
    public FileContent(Stream content, string contentType)
    {
        Content = content;
        ContentType = contentType;
    }


    public Stream Content { get; }
    public string ContentType { get; }
}
