using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using System.Security.Cryptography;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models.Options;

namespace Warp.WebApp.Services.Encryption;

/// <summary>
/// AES-256 implementation of the encryption service.
/// This service provides strong symmetric encryption using the Advanced Encryption Standard (AES) algorithm
/// with a 256-bit key length, CBC mode, and PKCS7 padding.
/// Encrypted data includes a header with a marker byte and version byte, followed by the initialization vector (IV),
/// and then the encrypted payload.
/// </summary>
public class AesEncryptionService : EncryptionServiceBase, IEncryptionService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AesEncryptionService"/> class
    /// </summary>
    /// <param name="featureManager">The feature manager to check if encryption is enabled</param>
    /// <param name="options">The encryption options containing the encryption key</param>
    public AesEncryptionService(IFeatureManager featureManager, IOptions<EncryptionOptions> options)
    {
        _encryptionKey = options.Value.EncryptionKey!;
        _isDisabled = !featureManager.IsEnabled("EntryEncryption");
    }


    /// <inheritdoc/>
    public ValueTask<byte[]> Encrypt(byte[] decryptedData)
    {
        if (decryptedData == null || decryptedData.Length == 0)
            return ValueTask.FromResult(Array.Empty<byte>());

        if (_isDisabled)
            return ValueTask.FromResult(decryptedData);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
            
        aes.GenerateIV();
            
        using var memoryStream = new MemoryStream();
        memoryStream.WriteByte(EncryptionMarker);
        memoryStream.WriteByte(EncryptionVersion);
        memoryStream.Write(aes.IV, 0, aes.IV.Length);
            
        using var encryptor = aes.CreateEncryptor();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            
        cryptoStream.Write(decryptedData, 0, decryptedData.Length);
        cryptoStream.FlushFinalBlock();
            
        return ValueTask.FromResult(memoryStream.ToArray());
    }


    /// <inheritdoc/>
    public ValueTask<byte[]> Decrypt(byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            return ValueTask.FromResult(Array.Empty<byte>());

        if (_isDisabled || !IsEncrypted(encryptedData))
            return ValueTask.FromResult(encryptedData);

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        
        var iv = new byte[aes.IV.Length];
        if (encryptedData.Length < iv.Length + EncryptionHeaderSize)
            throw new ArgumentException("Encrypted data is too short to contain an IV");
        
        Buffer.BlockCopy(encryptedData, EncryptionHeaderSize, iv, 0, iv.Length);
        aes.IV = iv;
        
        var encryptedSize = encryptedData.Length - iv.Length - EncryptionHeaderSize;
        var cipherText = new byte[encryptedSize];
        Buffer.BlockCopy(encryptedData, iv.Length + EncryptionHeaderSize, cipherText, 0, encryptedSize);
        
        using var memoryStream = new MemoryStream();
        using var decryptor = aes.CreateDecryptor();
        using var cryptoStream = new CryptoStream(new MemoryStream(cipherText), decryptor, CryptoStreamMode.Read);
        
        cryptoStream.CopyTo(memoryStream);
        
        return ValueTask.FromResult(memoryStream.ToArray());
    }

    
    private readonly byte[] _encryptionKey;
    private readonly bool _isDisabled;
}


// TODO: Investigate why all entries go to the ImageService database