using Warp.WebApp.Services;

namespace Warp.WebApp.Tests;

public class OpenGraphServiceTests
{
    [Fact]
    public void GetDefaultModel_ReturnsDefaultModel()
    {
        var result = OpenGraphService.GetDefaultModel(_description);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Title);
        Assert.NotEmpty(result.ImageUrl);
    }


    [Fact]
    public void GetModel_WithDescription_ReturnsModelWithProcessedDescription()
    {
        var result = OpenGraphService.GetModel(_description);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Title);
        Assert.Equal(_description, result.Description);
        Assert.NotEmpty(result.ImageUrl);
    }


    [Theory]
    [InlineData("This is a very long description that exceeds the maximum length allowed. It should be trimmed and end with ellipsis. very long description that exceeds the maximum length allowed. It should be trimmed and end with ellipsis.", "This is a very long description that exceeds the maximum length allowed. It should be trimmed and end with ellipsis. very long description that exceeds the maximum length allowed.")]
    [InlineData("This is a very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis, very long description that exceeds the maximum length allowed, it should be trimmed.", "This is a very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis, very long description that exceeds the maximum length allowed, it should be...")]
    [InlineData("This is a very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis, very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis.", "This is a very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis, very long description that exceeds the maximum length allowed, it should be...")]
    public void GetModel_WithLongDescription_ReturnsModelWithProcessedDescription(string description, string expected)
    {
        var result = OpenGraphService.GetModel(OpenGraphService.GetDescription(description));

        Assert.NotNull(result);
        Assert.Equal(expected, result.Description);
    }


    [Fact]
    public void GetModel_WithUrls_ReturnsModelWithProcessedUrl()
    {
        var urls = new List<string> { "https://example.com/image.jpg" };

        var result = OpenGraphService.GetModel(_description, urls);

        Assert.NotNull(result);
        Assert.Equal(urls[0], result.ImageUrl);
    }


    [Fact]
    public void GetModel_WithEmptyUrls_ReturnsModelWithDefaultImageUrl()
    {
        var urls = new List<string>();

        var result = OpenGraphService.GetModel(_description, urls);

        Assert.NotNull(result);
        Assert.NotEmpty(result.ImageUrl);
    }


    private const string _description = "This is a test description.";
}
