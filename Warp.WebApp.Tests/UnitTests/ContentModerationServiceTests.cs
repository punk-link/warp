using System.Net;
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Options;
using NSubstitute;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services.Moderation;

namespace Warp.WebApp.Tests.UnitTests;

public class ContentModerationServiceTests
{
    [Fact]
    public async Task ModerateText_ShouldUseConfiguredEndpointAndBearerToken()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new RecordingHandler();
        var httpClient = new HttpClient(handler);
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory
            .CreateClient(ContentModerationService.HttpClientName)
            .Returns(httpClient);

        var options = Options.Create(new ContentModerationOptions
        {
            Endpoint = "https://example.test/custom/v1",
            ApiKey = "test-api-key",
            Model = "omni-moderation-latest",
            InitialConcurrency = 1,
            MaxConcurrency = 1,
            SuccessThreshold = 0.95,
            SuccessRateWindow = TimeSpan.FromMinutes(1)
        });
        using var rateLimiter = new ContentModerationRateLimiter(options);
        var service = new ContentModerationService(httpClientFactory, rateLimiter, options);

        var result = await service.ModerateText("hello world", cancellationToken);

        Assert.True(result.IsFlagged);
        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal(new Uri("https://example.test/custom/v1/moderations"), handler.Request.RequestUri);
        Assert.Equal("Bearer", handler.Request.Headers.Authorization?.Scheme);
        Assert.Equal("test-api-key", handler.Request.Headers.Authorization?.Parameter);

        var body = await handler.Request.Content!.ReadAsStringAsync(cancellationToken);
        Assert.Contains("omni-moderation-latest", body, StringComparison.Ordinal);
        Assert.Contains("hello world", body, StringComparison.Ordinal);
    }


    private sealed class RecordingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }


        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    content: "{\"results\":[{\"flagged\":true,\"category_scores\":{\"violence\":0.9}}]}",
                    encoding: Encoding.UTF8,
                    mediaType: "application/json")
            };

            return Task.FromResult(response);
        }
    }
}