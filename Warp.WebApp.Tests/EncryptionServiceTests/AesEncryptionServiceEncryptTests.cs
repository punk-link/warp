using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using NSubstitute;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services.Encryption;

namespace Warp.WebApp.Tests.EncryptionServiceTests;

public class AesEncryptionServiceEncryptTests
{
    public AesEncryptionServiceEncryptTests()
    {
        _featureManagerSubstitute = Substitute.For<IFeatureManager>();
        _encryptionOptionsSubstitute = Substitute.For<IOptions<EncryptionOptions>>();
        
        _encryptionOptionsSubstitute.Value.Returns(new EncryptionOptions 
        { 
            EncryptionKey = new byte[32]
        });

        _encryptionService = new AesEncryptionService(_featureManagerSubstitute, _encryptionOptionsSubstitute);
    }


    [Fact]
    public void Encrypt_EmptyData_ReturnsEmptyArray()
    {
        byte[] data = [];
        
        var result = _encryptionService.Encrypt(data);
        
        Assert.Empty(result);
    }


    [Fact]
    public void Encrypt_NullData_ReturnsEmptyArray()
    {
        byte[]? data = null;
        
        var result = _encryptionService.Encrypt(data);
        
        Assert.Empty(result);
    }


    [Fact]
    public void Encrypt_EncryptionDisabled_ReturnsOriginalData()
    {
        _featureManagerSubstitute.IsEnabledAsync("EntryEncription").Returns(false);
        var encryptionService = new AesEncryptionService(_featureManagerSubstitute, _encryptionOptionsSubstitute);
        byte[] data = [1, 2, 3, 4, 5];
        
        var result = encryptionService.Encrypt(data);
        
        Assert.Equal(data, result);
    }


    [Fact]
    public void Encrypt_EncryptionEnabled_ReturnsEncryptedData()
    {
        _featureManagerSubstitute.IsEnabledAsync("EntryEncription").Returns(true);
        var encryptionService = new AesEncryptionService(_featureManagerSubstitute, _encryptionOptionsSubstitute);
        byte[] data = [1, 2, 3, 4, 5];
        
        var result = encryptionService.Encrypt(data);
        
        Assert.NotEmpty(result);
        Assert.NotEqual(data, result);
        Assert.True(result.Length > data.Length); // Encrypted data includes header and IV
        
        Assert.Equal(AesEncryptionService.EncryptionMarker, result[0]);
        Assert.Equal(AesEncryptionService.EncryptionVersion, result[1]);
    }


    [Fact]
    public void Encrypt_MultipleCalls_ProduceDifferentResults()
    {
        _featureManagerSubstitute.IsEnabledAsync("EntryEncription").Returns(true);
        var encryptionService = new AesEncryptionService(_featureManagerSubstitute, _encryptionOptionsSubstitute);
        byte[] data = [1, 2, 3, 4, 5];
        
        var result1 = encryptionService.Encrypt(data);
        var result2 = encryptionService.Encrypt(data);
        
        Assert.NotEqual(result1, result2); // Should be different due to different IVs
    }
    

    private readonly IOptions<EncryptionOptions> _encryptionOptionsSubstitute;
    private readonly AesEncryptionService _encryptionService;
    private readonly IFeatureManager _featureManagerSubstitute;
}