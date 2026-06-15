using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Warp.WebApp.Constants;
using Warp.WebApp.Helpers.Warmups;

namespace Warp.WebApp.Tests.UnitTests;

public class RouteWarmerServiceTests
{
    [Theory]
    [InlineData("http://localhost:8080", "/health", "http://localhost:8080/health")]
    [InlineData("http://localhost:8080/", "/health", "http://localhost:8080/health")]
    [InlineData("  http://localhost:8080  ", "/health", "http://localhost:8080/health")]
    [InlineData("http://localhost:8080", "health", "http://localhost:8080/health")]
    public async Task WarmUpRoutes_ShouldComposeRequestUriWithoutDuplicateSlashes(string baseUrl, string route, string expectedUri)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var handler = new RecordingHandler();
        var httpClient = new HttpClient(handler);
        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(HttpClients.Warmup).Returns(httpClient);
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var options = Options.Create(new RoutesWarmupOptions
        {
            BaseUrl = baseUrl,
            Routes = [route]
        });
        var service = new RouteWarmerService(httpClientFactory, loggerFactory, options);

        await service.WarmUpRoutes(cancellationToken);

        Assert.Equal(new Uri(expectedUri, UriKind.Absolute), handler.RequestUri);
    }


    private sealed class RecordingHandler : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }


        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}