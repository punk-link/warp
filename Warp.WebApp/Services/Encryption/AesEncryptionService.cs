using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using System.Security.Cryptography;
using Warp.WebApp.Models.Options;

namespace Warp.WebApp.Services.Encryption;

/// <summary>
/// AES-256 implementation of the encryption service
/// </summary>
public class AesEncryptionService : IEncryptionService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AesEncryptionService"/> class
    /// </summary>
    /// <param name="options">The encryption options</param>
    /// <param name="loggerFactory">The logger factory</param>
    public AesEncryptionService(IFeatureManager featureManager, IOptions<EncryptionOptions> options, ILoggerFactory loggerFactory)
    {
        _encryptionKey = options.Value.EncryptionKey!;
        _isDisabled = !featureManager.IsEnabledAsync("EntryEncription").GetAwaiter().GetResult();
        _logger = loggerFactory.CreateLogger<AesEncryptionService>();
    }


    /// <inheritdoc/>
    public byte[] Encrypt(byte[] decryptedData)
    {
        if (decryptedData == null || decryptedData.Length == 0)
            return [];

        if (_isDisabled)
            return decryptedData;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            aes.GenerateIV();
            
            using var memoryStream = new MemoryStream();
            memoryStream.WriteByte(EncriptionMarker);
            memoryStream.WriteByte(EncriptionVersion);
            memoryStream.Write(aes.IV, 0, aes.IV.Length);
            
            using var encryptor = aes.CreateEncryptor();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            
            cryptoStream.Write(decryptedData, 0, decryptedData.Length);
            cryptoStream.FlushFinalBlock();
            
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data");
            throw new CryptographicException("Failed to encrypt data", ex);
        }
    }


    /// <inheritdoc/>
    public byte[] Decrypt(byte[] encryptedData)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            return [];

        if (_isDisabled || !IsEncrypted(encryptedData))
            return encryptedData;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
        
            byte[] iv = new byte[aes.IV.Length];
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
        
            return memoryStream.ToArray();
        }
        catch (CryptographicException cryptoEx)
        {
            _logger.LogError(cryptoEx, "Cryptographic error while decrypting data");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data");
            throw new CryptographicException("Failed to decrypt data", ex);
        }
    }


    private static bool IsEncrypted(byte[] data)
    {
        // Quick length check - encrypted data must include header + IV (16 bytes) + some ciphertext
        const int minimumEncryptedLength = EncryptionHeaderSize + 16;
        if (data == null || data.Length < minimumEncryptedLength)
            return false;
    
        return data[0] == EncriptionMarker && data[1] == EncriptionVersion;
    }


    private const byte EncriptionMarker = 0xE5;
    private const byte EncriptionVersion = 0x01;
    private const int EncryptionHeaderSize = sizeof(byte) * 2;
    
    private readonly byte[] _encryptionKey;
    private readonly bool _isDisabled;
    private readonly ILogger<AesEncryptionService> _logger;
}

// TODO: Add proper error handling and logging

// TODO: Add Vault integration to store the encryption key

// TODO: Add unit tests for the encryption and decryption methods

// TODO: Investigate why all entries go to the ImageService database