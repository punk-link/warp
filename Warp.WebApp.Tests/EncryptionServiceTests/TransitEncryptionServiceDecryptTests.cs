using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using NSubstitute;
using System.Text;
using VaultSharp;
using VaultSharp.V1.Commons;
using VaultSharp.V1.SecretsEngines.Transit;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services.Encryption;
using Warp.WebApp.Tests.Infrastructure;

namespace Warp.WebApp.Tests.EncryptionServiceTests;

public class TransitEncryptionServiceDecryptTests
{
    public TransitEncryptionServiceDecryptTests()
    {
        _loggerFactory = Substitute.For<ILoggerFactory>();
        _logger = Substitute.For<ILogger<TransitEncryptionService>>();
        _loggerFactory.CreateLogger<TransitEncryptionService>().Returns(_logger);
        
        _featureManagerSubstitute = Substitute.For<IFeatureManager>();
        _featureManagerSubstitute.ConfigureFeature("EntryEncryption", false);

        _vaultClientSubstitute = Substitute.For<IVaultClient>();
        _encryptionOptionsSubstitute = Substitute.For<IOptions<EncryptionOptions>>();
        
        _encryptionOptionsSubstitute.Value.Returns(new EncryptionOptions 
        { 
            TransitKeyName = "test-key"
        });

        _encryptionService = new TransitEncryptionService(
            _loggerFactory,
            _featureManagerSubstitute, 
            _vaultClientSubstitute, 
            _encryptionOptionsSubstitute);
    }


    [Fact]
    public async Task Decrypt_EmptyData_ReturnsEmptyArray()
    {
        byte[] data = [];
        
        var result = await _encryptionService.Decrypt(data);
        
        Assert.Empty(result);
    }


    [Fact]
    public async Task Decrypt_NullData_ReturnsEmptyArray()
    {
        byte[]? data = null;
        
        var result = await _encryptionService.Decrypt(data);
        
        Assert.Empty(result);
    }


    [Fact]
    public async Task Decrypt_EncryptionDisabled_ReturnsOriginalData()
    {
        byte[] data = [1, 2, 3, 4, 5];
        
        var result = await _encryptionService.Decrypt(data);
        
        Assert.Equal(data, result);
    }


    [Fact]
    public async Task Decrypt_NonEncryptedData_ReturnsOriginalData()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new TransitEncryptionService(
            _loggerFactory,
            enabledFeatureManager, 
            _vaultClientSubstitute, 
            _encryptionOptionsSubstitute);
            
        byte[] data = [1, 2, 3, 4, 5];
        
        var result = await encryptionService.Decrypt(data);
        
        Assert.Equal(data, result);
    }


    [Fact]
    public async Task Decrypt_ValidEncryptedData_ReturnsDecryptedData()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new TransitEncryptionService(
            _loggerFactory,
            enabledFeatureManager, 
            _vaultClientSubstitute, 
            _encryptionOptionsSubstitute);

        byte[] originalData = [1, 2, 3, 4, 5];
        string base64PlainText = Convert.ToBase64String(originalData);
        string cipherText = "vault:v1:someCipherTextValue";
        
        var transitSecrets = Substitute.For<ITransitSecretsEngine>();
        
        // Create encrypted data by prepending header to ciphertext
        byte[] cipherTextBytes = Encoding.UTF8.GetBytes(cipherText);
        byte[] encryptedData = new byte[EncryptionServiceBase.EncryptionHeaderSize + cipherTextBytes.Length];
        encryptedData[0] = EncryptionServiceBase.EncryptionMarker;
        encryptedData[1] = EncryptionServiceBase.EncryptionVersion;
        Buffer.BlockCopy(cipherTextBytes, 0, encryptedData, EncryptionServiceBase.EncryptionHeaderSize, cipherTextBytes.Length);
        
        // Setup Vault client mock for decryption response
        var decryptResponse = new Secret<DecryptionResponse>
        {
            Data = new DecryptionResponse
            {
                Base64EncodedPlainText = base64PlainText
            }
        };
        
        transitSecrets.DecryptAsync(
            Arg.Is<string>(s => s == "test-key"),
            Arg.Is<DecryptRequestOptions>(options => options.CipherText == cipherText)
        ).Returns(decryptResponse);
        
        _vaultClientSubstitute.V1.Secrets.Transit.Returns(transitSecrets);
        
        var result = await encryptionService.Decrypt(encryptedData);
        
        Assert.Equal(originalData, result);
    }


    [Fact]
    public async Task Decrypt_NullPlainText_ThrowsInvalidOperationException()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new TransitEncryptionService(
            _loggerFactory,
            enabledFeatureManager, 
            _vaultClientSubstitute, 
            _encryptionOptionsSubstitute);

        string cipherText = "vault:v1:someCipherTextValue";
        
        var transitSecrets = Substitute.For<ITransitSecretsEngine>();
        
        // Create encrypted data by prepending header to ciphertext
        byte[] cipherTextBytes = Encoding.UTF8.GetBytes(cipherText);
        byte[] encryptedData = new byte[EncryptionServiceBase.EncryptionHeaderSize + cipherTextBytes.Length];
        encryptedData[0] = EncryptionServiceBase.EncryptionMarker;
        encryptedData[1] = EncryptionServiceBase.EncryptionVersion;
        Buffer.BlockCopy(cipherTextBytes, 0, encryptedData, EncryptionServiceBase.EncryptionHeaderSize, cipherTextBytes.Length);
        
        // Return null plaintext in response
        var decryptResponse = new Secret<DecryptionResponse>
        {
            Data = new DecryptionResponse
            {
                Base64EncodedPlainText = null
            }
        };
        
        transitSecrets.DecryptAsync(
            Arg.Any<string>(),
            Arg.Any<DecryptRequestOptions>()
        ).Returns(decryptResponse);
        
        _vaultClientSubstitute.V1.Secrets.Transit.Returns(transitSecrets);
        
        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await encryptionService.Decrypt(encryptedData));
    }


    [Fact]
    public async Task Decrypt_VaultError_ThrowsExceptionAndLogs()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new TransitEncryptionService(
            _loggerFactory,
            enabledFeatureManager, 
            _vaultClientSubstitute, 
            _encryptionOptionsSubstitute);

        string cipherText = "vault:v1:someCipherTextValue";
        
        var transitSecrets = Substitute.For<ITransitSecretsEngine>();
        
        // Create encrypted data by prepending header to ciphertext
        byte[] cipherTextBytes = Encoding.UTF8.GetBytes(cipherText);
        byte[] encryptedData = new byte[EncryptionServiceBase.EncryptionHeaderSize + cipherTextBytes.Length];
        encryptedData[0] = EncryptionServiceBase.EncryptionMarker;
        encryptedData[1] = EncryptionServiceBase.EncryptionVersion;
        Buffer.BlockCopy(cipherTextBytes, 0, encryptedData, EncryptionServiceBase.EncryptionHeaderSize, cipherTextBytes.Length);
        
        // Set up Vault client to throw an exception
        transitSecrets.DecryptAsync(
            Arg.Any<string>(),
            Arg.Any<DecryptRequestOptions>()
        ).Returns(Task.FromException<Secret<DecryptionResponse>>(new InvalidOperationException("Vault error")));
        
        _vaultClientSubstitute.V1.Secrets.Transit.Returns(transitSecrets);
        
        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await encryptionService.Decrypt(encryptedData));
    }


    [Fact]
    public async Task Decrypt_EncryptThenDecrypt_MaintainsDataIntegrity()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new TransitEncryptionService(
            _loggerFactory,
            enabledFeatureManager, 
            _vaultClientSubstitute, 
            _encryptionOptionsSubstitute);
        
        byte[] originalData = [1, 2, 3, 4, 5];
        string cipherText = "vault:v1:encryptedValue";
        string base64PlainText = Convert.ToBase64String(originalData);
        
        var transitSecrets = Substitute.For<ITransitSecretsEngine>();
        
        // Setup for encrypt
        var encryptResponse = new Secret<EncryptionResponse>
        {
            Data = new EncryptionResponse
            {
                CipherText = cipherText
            }
        };
        
        transitSecrets.EncryptAsync(
            Arg.Is<string>(s => s == "test-key"),
            Arg.Any<EncryptRequestOptions>()
        ).Returns(encryptResponse);
        
        // Setup for decrypt
        var decryptResponse = new Secret<DecryptionResponse>
        {
            Data = new DecryptionResponse
            {
                Base64EncodedPlainText = base64PlainText
            }
        };
        
        transitSecrets.DecryptAsync(
            Arg.Is<string>(s => s == "test-key"),
            Arg.Is<DecryptRequestOptions>(options => options.CipherText == cipherText)
        ).Returns(decryptResponse);
        
        _vaultClientSubstitute.V1.Secrets.Transit.Returns(transitSecrets);
        
        var encryptedData = await encryptionService.Encrypt(originalData);
        var decryptedData = await encryptionService.Decrypt(encryptedData);
        
        Assert.Equal(originalData, decryptedData);
    }


    private readonly IOptions<EncryptionOptions> _encryptionOptionsSubstitute;
    private readonly TransitEncryptionService _encryptionService;
    private readonly IFeatureManager _featureManagerSubstitute;
    private readonly ILogger<TransitEncryptionService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IVaultClient _vaultClientSubstitute;
}