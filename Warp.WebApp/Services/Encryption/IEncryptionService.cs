namespace Warp.WebApp.Services.Encryption;

/// <summary>
/// Service for encrypting and decrypting data
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts the specified data
    /// </summary>
    /// <param name="decryptedData">The data to encrypt</param>
    /// <returns>
    /// A byte array containing the encrypted data with header and IV prepended,
    /// or the original data if encryption is disabled or the input is empty.
    /// </returns>
    ValueTask<byte[]> Encrypt(byte[] decryptedData);
    
    /// <summary>
    /// Decrypts the specified data
    /// </summary>
    /// <param name="encryptedData">The encrypted data</param>
    /// <returns>The decrypted data</returns>
    ValueTask<byte[]> Decrypt(byte[] encryptedData);
}