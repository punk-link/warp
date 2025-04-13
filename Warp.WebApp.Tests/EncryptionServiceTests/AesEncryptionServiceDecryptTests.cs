using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using NSubstitute;
using System.Security.Cryptography;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services.Encryption;

namespace Warp.WebApp.Tests.EncryptionServiceTests;

public class AesEncryptionServiceDecryptTests
{
    public AesEncryptionServiceDecryptTests()
    {
        _featureManagerSubstitute = Substitute.For<IFeatureManager>();
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
    public void Decrypt_EmptyData_ReturnsEmptyArray()
    {
        byte[] data = [];
        
        var result = _encryptionService.Decrypt(data);
        
        Assert.Empty(result);
    }


    [Fact]
    public void Decrypt_NullData_ReturnsEmptyArray()
    {
        byte[]? data = null;
        
        var result = _encryptionService.Decrypt(data);
        
        Assert.Empty(result);
    }


    [Fact]
    public void Decrypt_EncryptionDisabled_ReturnsOriginalData()
    {
        _featureManagerSubstitute.IsEnabledAsync("EntryEncription").Returns(false);
        var encryptionService = new AesEncryptionService(_featureManagerSubstitute, _encryptionOptionsSubstitute);
        byte[] data = [1, 2, 3, 4, 5];
        
        var result = encryptionService.Decrypt(data);
        
        Assert.Equal(data, result);
    }


    [Fact]
    public void Decrypt_NonEncryptedData_ReturnsOriginalData()
    {
        _featureManagerSubstitute.IsEnabledAsync("EntryEncription").Returns(true);
        var encryptionService = new AesEncryptionService(_featureManagerSubstitute, _encryptionOptionsSubstitute);
        byte[] data = [1, 2, 3, 4, 5];
        
        var result = encryptionService.Decrypt(data);
        
        Assert.Equal(data, result);
    }


    [Fact]
    public void Decrypt_InvalidEncryptedData_ThrowsCryptographicException()
    {
        _featureManagerSubstitute.IsEnabledAsync("EntryEncription").Returns(true);
        var encryptionService = new AesEncryptionService(_featureManagerSubstitute, _encryptionOptionsSubstitute);
        
        byte[] data = new byte[20];
        data[0] = AesEncryptionService.EncryptionMarker;
        data[1] = AesEncryptionService.EncryptionVersion;
        
        Assert.Throws<CryptographicException>(() => encryptionService.Decrypt(data));
    }


    [Fact]
    public void Decrypt_ValidEncryptedData_ReturnsDecryptedData()
    {
        _featureManagerSubstitute.IsEnabledAsync("EntryEncription").Returns(true);
        var encryptionService = new AesEncryptionService(_featureManagerSubstitute, _encryptionOptionsSubstitute);
        byte[] originalData = [1, 2, 3, 4, 5];
        
        var encryptedData = encryptionService.Encrypt(originalData);
        var decryptedData = encryptionService.Decrypt(encryptedData);
        
        Assert.Equal(originalData, decryptedData);
    }


    [Fact]
    public void Decrypt_EncryptionThenDecryption_MaintainsDataIntegrity()
    {
        _featureManagerSubstitute.IsEnabledAsync("EntryEncription").Returns(true);
        var encryptionService = new AesEncryptionService(_featureManagerSubstitute, _encryptionOptionsSubstitute);
        
        byte[] originalData = new byte[1000];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(originalData);
        
        var encryptedData = encryptionService.Encrypt(originalData);
        var decryptedData = encryptionService.Decrypt(encryptedData);
        
        Assert.Equal(originalData, decryptedData);
    }
    

    private readonly IOptions<EncryptionOptions> _encryptionOptionsSubstitute;
    private readonly AesEncryptionService _encryptionService;
    private readonly IFeatureManager _featureManagerSubstitute;
}