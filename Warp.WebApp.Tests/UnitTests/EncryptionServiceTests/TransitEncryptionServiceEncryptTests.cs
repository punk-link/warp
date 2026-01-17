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

namespace Warp.WebApp.Tests.UnitTests.EncryptionServiceTests;

public class TransitEncryptionServiceEncryptTests
{
    public TransitEncryptionServiceEncryptTests()
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
    public async Task Encrypt_EmptyData_ReturnsEmptyArray()
    {
        byte[] data = [];
        
        var result = await _encryptionService.Encrypt(data);
        
        Assert.Empty(result);
    }


    [Fact]
    public async Task Encrypt_NullData_ReturnsEmptyArray()
    {
        byte[]? data = null;
        
        var result = await _encryptionService.Encrypt(data!);
        
        Assert.Empty(result);
    }


    [Fact]
    public async Task Encrypt_EncryptionDisabled_ReturnsOriginalData()
    {
        byte[] data = [1, 2, 3, 4, 5];
        
        var result = await _encryptionService.Encrypt(data);
        
        Assert.Equal(data, result);
    }


    [Fact]
    public async Task Encrypt_EncryptionEnabled_ReturnsEncryptedData()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new TransitEncryptionService(_loggerFactory, enabledFeatureManager, _vaultClientSubstitute, _encryptionOptionsSubstitute);

        byte[] data = [1, 2, 3, 4, 5];
        string cipherText = "vault:v1:someCipherTextValue";
        
        var transitSecrets = Substitute.For<ITransitSecretsEngine>();
        var transitResponse = new Secret<EncryptionResponse>
        {
            Data = new EncryptionResponse
            {
                CipherText = cipherText
            }
        };
        
        transitSecrets.EncryptAsync(
            Arg.Is<string>(s => s == "test-key"),
            Arg.Any<EncryptRequestOptions>()
        ).Returns(transitResponse);
        
        _vaultClientSubstitute.V1.Secrets.Transit.Returns(transitSecrets);
        
        var result = await encryptionService.Encrypt(data);
        
        Assert.NotEmpty(result);
        Assert.NotEqual(data, result);
        Assert.True(result.Length > cipherText.Length); // Encrypted data includes header + ciphertext
        
        Assert.Equal(EncryptionServiceBase.EncryptionMarker, result[0]);
        Assert.Equal(EncryptionServiceBase.EncryptionVersion, result[1]);
        
        // Verify the ciphertext is included
        byte[] extractedCipherText = new byte[result.Length - 2];
        Buffer.BlockCopy(result, 2, extractedCipherText, 0, extractedCipherText.Length);
        Assert.Equal(cipherText, Encoding.UTF8.GetString(extractedCipherText));
    }


    [Fact]
    public async Task Encrypt_VaultError_ThrowsException()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new TransitEncryptionService(_loggerFactory, enabledFeatureManager, _vaultClientSubstitute, _encryptionOptionsSubstitute);

        byte[] data = [1, 2, 3, 4, 5];
        
        var transitSecrets = Substitute.For<ITransitSecretsEngine>();
        transitSecrets.EncryptAsync(
            Arg.Any<string>(),
            Arg.Any<EncryptRequestOptions>()
        ).Returns(Task.FromException<Secret<EncryptionResponse>>(new InvalidOperationException("Vault error")));
        
        _vaultClientSubstitute.V1.Secrets.Transit.Returns(transitSecrets);
        
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await encryptionService.Encrypt(data));
    }


    [Fact]
    public async Task Encrypt_NullCipherText_ThrowsInvalidOperationException()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new TransitEncryptionService(_loggerFactory, enabledFeatureManager, _vaultClientSubstitute, _encryptionOptionsSubstitute);
        
        byte[] data = [1, 2, 3, 4, 5];
        
        var transitSecrets = Substitute.For<ITransitSecretsEngine>();
        var transitResponse = new Secret<EncryptionResponse>
        {
            Data = new EncryptionResponse
            {
                CipherText = null
            }
        };
        
        transitSecrets.EncryptAsync(
            Arg.Any<string>(),
            Arg.Any<EncryptRequestOptions>()
        ).Returns(transitResponse);
        
        _vaultClientSubstitute.V1.Secrets.Transit.Returns(transitSecrets);
        
        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await encryptionService.Encrypt(data));
    }


    private readonly IOptions<EncryptionOptions> _encryptionOptionsSubstitute;
    private readonly TransitEncryptionService _encryptionService;
    private readonly IFeatureManager _featureManagerSubstitute;
    private readonly ILogger<TransitEncryptionService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IVaultClient _vaultClientSubstitute;
}