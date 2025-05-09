using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using System.Text;
using VaultSharp;
using VaultSharp.V1.SecretsEngines.Transit;
using Warp.WebApp.Extensions;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Telemetry.Logging;

namespace Warp.WebApp.Services.Encryption;

/// <summary>
/// Vault Transit Secret Engine implementation of the encryption service.
/// This service uses HashiCorp Vault's Transit Secret Engine for encryption operations.
/// Encrypted data includes a header with a marker byte and version byte, followed by the ciphertext.
/// For fallback compatibility, it can also decrypt data encrypted with AES-256.
/// </summary>
public class TransitEncryptionService : EncryptionServiceBase, IEncryptionService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransitEncryptionService"/> class
    /// </summary>
    /// <param name="loggerFactory">The logger factory for logging errors</param>
    /// <param name="featureManager">The feature manager to check if encryption is enabled</param>
    /// <param name="vaultClient">The Vault client for transit operations</param>
    /// <param name="options">The encryption options containing the transit key name</param>
    public TransitEncryptionService(ILoggerFactory loggerFactory, IFeatureManager featureManager, IVaultClient vaultClient, IOptions<EncryptionOptions> options)
    {
        _isDisabled = !featureManager.IsEnabled("EntryEncryption");
        _keyName = options.Value.TransitKeyName!;
        _logger = loggerFactory.CreateLogger<TransitEncryptionService>();
        _vaultClient = vaultClient;
    }


    /// <inheritdoc/>
    public async ValueTask<byte[]> Encrypt(byte[] decryptedData)
    {
        try
        {
            if (decryptedData == null || decryptedData.Length == 0)
                return [];

            if (_isDisabled)
                return decryptedData;

            var response = await _vaultClient.V1.Secrets.Transit.EncryptAsync(_keyName, new EncryptRequestOptions
            {
                Base64EncodedPlainText = Convert.ToBase64String(decryptedData)
            });

            var ciphertext = response.Data.CipherText;
            if (string.IsNullOrEmpty(ciphertext))
                throw new InvalidOperationException("Failed to get ciphertext from Vault transit encryption response");

            // Store the ciphertext with our encryption marker/version
            // This ensures compatibility with AesEncryptionService format
            var ciphertextBytes = Encoding.UTF8.GetBytes(ciphertext);
            var result = new byte[EncryptionHeaderSize + ciphertextBytes.Length];
            result[0] = EncryptionMarker;
            result[1] = EncryptionVersion;
            Buffer.BlockCopy(ciphertextBytes, 0, result, EncryptionHeaderSize, ciphertextBytes.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogVaultCiphertextError(ex.Message);
            throw;
        }
    }


    /// <inheritdoc/>
    public async ValueTask<byte[]> Decrypt(byte[] encryptedData)
    {
        try
        {
            if (encryptedData == null || encryptedData.Length == 0)
                return [];

            if (_isDisabled || !IsEncrypted(encryptedData))
                return encryptedData;

            var ciphertextBytes = new byte[encryptedData.Length - EncryptionHeaderSize];
            Buffer.BlockCopy(encryptedData, EncryptionHeaderSize, ciphertextBytes, 0, ciphertextBytes.Length);
            var ciphertext = Encoding.UTF8.GetString(ciphertextBytes);

            var response = await _vaultClient.V1.Secrets.Transit.DecryptAsync(_keyName, new DecryptRequestOptions
            {
                CipherText = ciphertext,
            });
        
            var base64EncodedPlaintext = response.Data.Base64EncodedPlainText;
            if (string.IsNullOrEmpty(base64EncodedPlaintext))
                throw new InvalidOperationException("Failed to get plaintext from Vault transit decryption response");

            return Convert.FromBase64String(base64EncodedPlaintext);
        }
        catch (Exception ex) {
            _logger.LogVaultPlaintextError(ex.Message);
            throw;
        }
    }


    private readonly bool _isDisabled;
    private readonly string _keyName;
    private readonly ILogger _logger;
    private readonly IVaultClient _vaultClient;
}