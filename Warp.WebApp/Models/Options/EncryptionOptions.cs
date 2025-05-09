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

    /// <summary>
    /// Gets or sets the name of the transit key in Vault
    /// </summary>
    public string? TransitKeyName { get; set; } = "warp-keydb";
}