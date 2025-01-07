using Microsoft.Extensions.Localization;
using NSubstitute;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Infrastructure;
using Warp.WebApp.Services.OpenGraph;

namespace Warp.WebApp.Tests;

public class OpenGraphServiceTests
{
    public OpenGraphServiceTests()
    {
        var localizerMock = Substitute.For<IStringLocalizer<ServerResources>>();

        localizerMock["DefaultOpenGraphDescriptionText"].Returns(new LocalizedString("DefaultOpenGraphDescriptionText", DefaultDescription));

        _localizer = localizerMock;
    }


    [Fact]
    public void GetDefaultDescription_ReturnsDefaultDescription()
    {
        var service = new OpenGraphService(_localizer);

        var result = service.GetDefaultDescription();

        Assert.NotEmpty(result.Title);
        Assert.Equal(DefaultDescription, result.Description);
        Assert.NotNull(result.ImageUrl);
    }


    [Fact]
    public void BuildDescription_WithDescription_ReturnsProcessedDescription()
    {
        var service = new OpenGraphService(_localizer);

        var result = service.BuildDescription(Description, DefaultUri);

        Assert.NotEmpty(result.Title);
        Assert.Equal(Description, result.Description);
        Assert.NotNull(result.ImageUrl);
    }


    [Theory]
    [InlineData("This is a very long description that exceeds the maximum length allowed. It should be trimmed and end with ellipsis. very long description that exceeds the maximum length allowed. It should be trimmed and end with ellipsis.", "This is a very long description that exceeds the maximum length allowed. It should be trimmed and end with ellipsis. very long description that exceeds the maximum length allowed.")]
    [InlineData("This is a very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis, very long description that exceeds the maximum length allowed, it should be trimmed.", "This is a very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis, very long description that exceeds the maximum length allowed, it should be...")]
    [InlineData("This is a very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis, very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis.", "This is a very long description that exceeds the maximum length allowed, it should be trimmed and end with ellipsis, very long description that exceeds the maximum length allowed, it should be...")]
    public void BuildDescription_WithLongDescription_ReturnsProcessedDescription(string description, string expected)
    {
        var service = new OpenGraphService(_localizer);
        var result = service.BuildDescription(description, DefaultUri);

        Assert.Equal(expected, result.Description);
    }


    private const string DefaultDescription = "DefaultOpenGraphDescriptionText";
    private const string Description = "This is a test description.";
    private Uri DefaultUri = new("https://example.com/image.jpg");

    private readonly IStringLocalizer<ServerResources> _localizer;
}
