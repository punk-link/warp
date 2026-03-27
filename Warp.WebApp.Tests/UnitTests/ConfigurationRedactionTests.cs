using Warp.WebApp.Helpers.Configuration;

namespace Warp.WebApp.Tests.UnitTests;

public class ConfigurationRedactionTests
{
    [Fact]
    public void BuildRedactedConfigData_WithSensitiveKeys_RedactsValues()
    {
        var data = new Dictionary<string, string?>
        {
            ["Database:Password"] = "super-secret",
            ["Vault:Token"] = "vault-token-value",
            ["Api:SecretKey"] = "my-api-secret",
        };

        var result = ConfigurationLogHelper.BuildRedactedConfigData(data);

        Assert.Equal("Database:Password=[REDACTED], Vault:Token=[REDACTED], Api:SecretKey=[REDACTED]", result);
    }


    [Fact]
    public void BuildRedactedConfigData_WithNonSensitiveKeys_PreservesValues()
    {
        var data = new Dictionary<string, string?>
        {
            ["ServiceName"] = "warp",
            ["Database:Port"] = "5432",
        };

        var result = ConfigurationLogHelper.BuildRedactedConfigData(data);

        Assert.Equal("ServiceName=warp, Database:Port=5432", result);
    }


    [Fact]
    public void BuildRedactedConfigData_WithMixedKeys_RedactsSensitiveOnly()
    {
        var data = new Dictionary<string, string?>
        {
            ["ServiceName"] = "warp",
            ["Database:ConnectionString"] = "Server=localhost;Password=123",
            ["Database:Port"] = "5432",
        };

        var result = ConfigurationLogHelper.BuildRedactedConfigData(data);

        Assert.Equal("ServiceName=warp, Database:ConnectionString=[REDACTED], Database:Port=5432", result);
    }


    [Fact]
    public void BuildRedactedConfigData_WithEmptyData_ReturnsEmptyString()
    {
        var data = new Dictionary<string, string?>();

        var result = ConfigurationLogHelper.BuildRedactedConfigData(data);

        Assert.Equal(string.Empty, result);
    }


    [Fact]
    public void BuildRedactedConfigData_WithDeeplyNestedSensitiveKey_RedactsValue()
    {
        var data = new Dictionary<string, string?>
        {
            ["Section:SubSection:ConnectionString"] = "Server=localhost",
        };

        var result = ConfigurationLogHelper.BuildRedactedConfigData(data);

        Assert.Equal("Section:SubSection:ConnectionString=[REDACTED]", result);
    }


    [Theory]
    [InlineData("SECRET")]
    [InlineData("Password")]
    [InlineData("TOKEN")]
    [InlineData("Credential")]
    public void BuildRedactedConfigData_CaseInsensitiveMatching_RedactsValue(string key)
    {
        var data = new Dictionary<string, string?>
        {
            [key] = "sensitive-value",
        };

        var result = ConfigurationLogHelper.BuildRedactedConfigData(data);

        Assert.Equal($"{key}=[REDACTED]", result);
    }
}
