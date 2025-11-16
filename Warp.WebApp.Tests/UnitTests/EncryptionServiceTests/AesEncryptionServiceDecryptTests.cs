using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using NSubstitute;
using System.Security.Cryptography;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services.Encryption;
using Warp.WebApp.Tests.Infrastructure;

namespace Warp.WebApp.Tests.UnitTests.EncryptionServiceTests;

public class AesEncryptionServiceDecryptTests
{
    public AesEncryptionServiceDecryptTests()
    {
        _featureManagerSubstitute = Substitute.For<IFeatureManager>();
        _featureManagerSubstitute.ConfigureFeature("EntryEncryption", false);

        _encryptionOptionsSubstitute = Substitute.For<IOptions<EncryptionOptions>>();
        
        var key = new byte[32];
        for (int i = 0; i < key.Length; i++)
            key[i] = (byte)i;
        
        _encryptionOptionsSubstitute.Value.Returns(new EncryptionOptions 
        { 
            EncryptionKey = key
        });

        _encryptionService = new AesEncryptionService(_featureManagerSubstitute, _encryptionOptionsSubstitute);
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
        
        var result = await _encryptionService.Decrypt(data!);
        
        Assert.Empty(result);
    }


    [Fact]
    public async Task Decrypt_EncryptionDisabled_ReturnsOriginalData()
    {
        var encryptionService = new AesEncryptionService(_featureManagerSubstitute, _encryptionOptionsSubstitute);
        byte[] data = [1, 2, 3, 4, 5];
        
        var result = await encryptionService.Decrypt(data);
        
        Assert.Equal(data, result);
    }


    [Fact]
    public async Task Decrypt_NonEncryptedData_ReturnsOriginalData()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new AesEncryptionService(enabledFeatureManager, _encryptionOptionsSubstitute);
        byte[] data = [1, 2, 3, 4, 5];
        
        var result = await encryptionService.Decrypt(data);
        
        Assert.Equal(data, result);
    }


    [Fact]
    public async Task Decrypt_InvalidEncryptedData_ThrowsCryptographicException()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new AesEncryptionService(enabledFeatureManager, _encryptionOptionsSubstitute);
        
        byte[] data = new byte[20];
        data[0] = EncryptionServiceBase.EncryptionMarker;
        data[1] = EncryptionServiceBase.EncryptionVersion;
        
        await Assert.ThrowsAsync<CryptographicException>(async () => await encryptionService.Decrypt(data));
    }


    [Fact]
    public async Task Decrypt_ValidEncryptedData_ReturnsDecryptedData()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new AesEncryptionService(enabledFeatureManager, _encryptionOptionsSubstitute);
        byte[] originalData = [1, 2, 3, 4, 5];
        
        var encryptedData = await encryptionService.Encrypt(originalData);
        var decryptedData = await encryptionService.Decrypt(encryptedData);
        
        Assert.Equal(originalData, decryptedData);
    }


    [Fact]
    public async Task Decrypt_EncryptionThenDecryption_MaintainsDataIntegrity()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new AesEncryptionService(enabledFeatureManager, _encryptionOptionsSubstitute);
        
        byte[] originalData = new byte[1000];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(originalData);
        
        var encryptedData = await encryptionService.Encrypt(originalData);
        var decryptedData = await encryptionService.Decrypt(encryptedData);
        
        Assert.Equal(originalData, decryptedData);
    }
    

    private readonly IOptions<EncryptionOptions> _encryptionOptionsSubstitute;
    private readonly AesEncryptionService _encryptionService;
    private readonly IFeatureManager _featureManagerSubstitute;
}