namespace Warp.WebApp.Models.Options;

/// <summary>
/// Configuration options for data encryption
/// </summary>
public class EncryptionOptions
{
    /// <summary>
    /// Gets or sets the encryption key
    /// </summary>
    public byte[]? EncryptionKey { get; set; }
}