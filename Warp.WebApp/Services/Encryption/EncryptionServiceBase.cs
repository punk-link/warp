namespace Warp.WebApp.Services.Encryption;

public abstract class EncryptionServiceBase
{
    /// <summary>
    /// Marker byte used to identify encrypted data
    /// </summary>
    public const byte EncryptionMarker = 0xE5;

    /// <summary>
    /// Version byte for the encryption format
    /// </summary>
    public const byte EncryptionVersion = 0x01;

    
    protected static bool IsEncrypted(byte[] data)
    {
        // Quick length check - encrypted data must include header + some ciphertext
        const int minimumEncryptedLength = EncryptionHeaderSize + 16; // assuming at least 16 bytes of ciphertext
        if (data == null || data.Length < minimumEncryptedLength)
            return false;
    
        return data[0] == EncryptionMarker && data[1] == EncryptionVersion;
    }


    public const int EncryptionHeaderSize = sizeof(byte) * 2;
}
