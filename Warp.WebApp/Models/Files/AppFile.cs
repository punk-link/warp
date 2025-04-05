namespace Warp.WebApp.Models.Files;

public readonly record struct AppFile
{
    public AppFile(Stream content, string contentMimeType, string untrustedFileName, string hash)
    {
        Content = content;
        ContentMimeType = contentMimeType;
        Hash = hash;
        UntrustedFileName = untrustedFileName;
    }


    public AppFile(Stream content, string contentMimeType, string untrustedFileName)
        : this(content, contentMimeType, untrustedFileName, string.Empty)
    {
    }


    public AppFile(Stream content, string contentMimeType)
        : this(content, contentMimeType, string.Empty, string.Empty)
    {
    }


    public static AppFile AddHash(AppFile appFile, string hash)
        => new (appFile.Content, appFile.ContentMimeType, appFile.UntrustedFileName, hash);


    public Stream Content { get; }
    public string ContentMimeType { get; }
    public string Hash { get; }
    public string UntrustedFileName { get; }
}
