using Microsoft.Extensions.Localization;
using NSubstitute;
using Warp.WebApp.Models;
using Warp.WebApp.Models.Entries;
using Warp.WebApp.Models.Entries.Enums;
using Warp.WebApp.Services;

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
        var entry = new Entry(Guid.NewGuid(), Description, DateTime.Now, DateTime.Now, EditMode.Unknown, [], EntryOpenGraphDescription.Empty);

        var result = service.BuildDescription(entry);

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
        var entry = new Entry(Guid.NewGuid(), description, DateTime.Now, DateTime.Now, EditMode.Unknown, [], EntryOpenGraphDescription.Empty);

        var result = service.BuildDescription(entry);

        Assert.Equal(expected, result.Description);
    }


    [Fact]
    public void BuildDescription_WithUrls_ReturnsModelWithProcessedUrl()
    {
        var urls = new List<Uri> { new("https://example.com/image.jpg") };
        var service = new OpenGraphService(_localizer);
        var entry = new Entry(Guid.NewGuid(), Description, DateTime.Now, DateTime.Now, EditMode.Unknown, urls, EntryOpenGraphDescription.Empty);

        var result = service.BuildDescription(entry);

        Assert.Equal(urls[0], result.ImageUrl);
    }


    [Fact]
    public void BuildDescription_WithEmptyUrls_ReturnsModelWithDefaultImageUrl()
    {
        var urls = new List<Uri>();
        var service = new OpenGraphService(_localizer);
        var entry = new Entry(Guid.NewGuid(), Description, DateTime.Now, DateTime.Now, EditMode.Unknown, urls, EntryOpenGraphDescription.Empty);

        var result = service.BuildDescription(entry);

        Assert.NotNull(result.ImageUrl);
    }


    private const string DefaultDescription = "DefaultOpenGraphDescriptionText";
    private const string Description = "This is a test description.";

    private readonly IStringLocalizer<ServerResources> _localizer;
}
