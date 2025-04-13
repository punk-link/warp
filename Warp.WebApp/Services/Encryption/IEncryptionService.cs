namespace Warp.WebApp.Services.Encryption;

/// <summary>
/// Service for encrypting and decrypting data
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts the specified data
    /// </summary>
    /// <param name="data">The data to encrypt</param>
        /// <returns>The encrypted data</returns>
    byte[] Encrypt(byte[] data);
    
    /// <summary>
    /// Decrypts the specified data
    /// </summary>
    /// <param name="encryptedData">The encrypted data</param>
        /// <returns>The decrypted data</returns>
    byte[] Decrypt(byte[] encryptedData);
}