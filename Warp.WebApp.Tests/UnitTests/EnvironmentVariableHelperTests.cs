using Warp.WebApp.Helpers.Configuration;

namespace Warp.WebApp.Tests.UnitTests;

public class EnvironmentVariableHelperNormalizeTests
{
    [Theory]
    [InlineData("value", "value")]
    [InlineData(" value", "value")]
    [InlineData("value ", "value")]
    [InlineData("  value  ", "value")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData(null, "")]
    public void Normalize_ShouldTrimWhitespace(string? value, string expected)
    {
        var result = EnvironmentVariableHelper.Normalize(value);

        Assert.Equal(expected, result);
    }
}


public class EnvironmentVariableHelperNormalizeUrlTests
{
    [Theory]
    [InlineData("https://example.test", "https://example.test/")]
    [InlineData("https://example.test/", "https://example.test/")]
    [InlineData("https://example.test///", "https://example.test/")]
    [InlineData("  https://example.test/path  ", "https://example.test/path/")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData(null, "")]
    public void NormalizeUrl_ShouldTrimWhitespaceAndEnsureSingleTrailingSlash(string? value, string expected)
    {
        var result = EnvironmentVariableHelper.NormalizeUrl(value);

        Assert.Equal(expected, result);
    }
}