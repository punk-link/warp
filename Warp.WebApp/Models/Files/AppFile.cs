namespace Warp.WebApp.Models.Files;

public readonly record struct AppFile
{
    public AppFile(Stream content, string contentMimeType, string untrustedFileName)
    {
        Content = content;
        ContentMimeType = contentMimeType;
        UntrustedFileName = untrustedFileName;
    }


    public AppFile(Stream content, string contentMimeType)
        : this(content, contentMimeType, string.Empty)
    {
    }


    public Stream Content { get; }
    public string ContentMimeType { get; }
    public string UntrustedFileName { get; }
}
