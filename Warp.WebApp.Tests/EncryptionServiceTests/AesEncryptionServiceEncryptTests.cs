using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using NSubstitute;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services.Encryption;
using Warp.WebApp.Tests.Infrastructure;

namespace Warp.WebApp.Tests.EncryptionServiceTests;

public class AesEncryptionServiceEncryptTests
{
    public AesEncryptionServiceEncryptTests()
    {
        _featureManagerSubstitute = Substitute.For<IFeatureManager>();
        _featureManagerSubstitute.ConfigureFeature("EntryEncryption", false);

        _encryptionOptionsSubstitute = Substitute.For<IOptions<EncryptionOptions>>();
        
        _encryptionOptionsSubstitute.Value.Returns(new EncryptionOptions 
        { 
            EncryptionKey = new byte[32]
        });

        _encryptionService = new AesEncryptionService(_featureManagerSubstitute, _encryptionOptionsSubstitute);
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
        var encryptionService = new AesEncryptionService(_featureManagerSubstitute, _encryptionOptionsSubstitute);
        byte[] data = [1, 2, 3, 4, 5];
        
        var result = await encryptionService.Encrypt(data);
        
        Assert.Equal(data, result);
    }


    [Fact]
    public async Task Encrypt_EncryptionEnabled_ReturnsEncryptedData()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new AesEncryptionService(enabledFeatureManager, _encryptionOptionsSubstitute);
        byte[] data = [1, 2, 3, 4, 5];
        
        var result = await encryptionService.Encrypt(data);
        
        Assert.NotEmpty(result);
        Assert.NotEqual(data, result);
        Assert.True(result.Length > data.Length); // Encrypted data includes header and IV
        
        Assert.Equal(EncryptionServiceBase.EncryptionMarker, result[0]);
        Assert.Equal(EncryptionServiceBase.EncryptionVersion, result[1]);
    }


    [Fact]
    public async Task Encrypt_MultipleCalls_ProduceDifferentResults()
    {
        var enabledFeatureManager = _featureManagerSubstitute.ConfigureFeature("EntryEncryption", true);
        var encryptionService = new AesEncryptionService(enabledFeatureManager, _encryptionOptionsSubstitute);
        byte[] data = [1, 2, 3, 4, 5];
        
        var result1 = await encryptionService.Encrypt(data);
        var result2 = await encryptionService.Encrypt(data);
        
        Assert.NotEqual(result1, result2); // Should be different due to different IVs
    }
    

    private readonly IOptions<EncryptionOptions> _encryptionOptionsSubstitute;
    private readonly AesEncryptionService _encryptionService;
    private readonly IFeatureManager _featureManagerSubstitute;
}