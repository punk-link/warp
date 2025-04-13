using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
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
    public AesEncryptionService(IOptions<EncryptionOptions> options, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<AesEncryptionService>();
        _encryptionKey = options.Value.EncryptionKey!;
    }


    /// <inheritdoc/>
    public byte[] Encrypt(byte[] data)
    {
        if (data == null || data.Length == 0)
            return [];

        try
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            aes.GenerateIV();
            
            using var memoryStream = new MemoryStream();
            memoryStream.Write(aes.IV, 0, aes.IV.Length);
            
            using var encryptor = aes.CreateEncryptor();
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            
            cryptoStream.Write(data, 0, data.Length);
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

        try
        {
            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            
            byte[] iv = new byte[aes.IV.Length];
            if (encryptedData.Length < iv.Length)
                throw new ArgumentException("Encrypted data is too short to contain an IV");
            
            Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
            aes.IV = iv;
            
            var encryptedSize = encryptedData.Length - iv.Length;
            var cipherText = new byte[encryptedSize];
            Buffer.BlockCopy(encryptedData, iv.Length, cipherText, 0, encryptedSize);
            
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

    
    private readonly byte[] _encryptionKey;
    private readonly ILogger<AesEncryptionService> _logger;
}