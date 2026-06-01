using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Warp.WebApp.Models.Moderation;
using Warp.WebApp.Models.Options;

namespace Warp.WebApp.Services.Moderation;

/// <summary>
/// Calls the OpenAI Moderation API to assess text and image content.
/// Uses the named <c>ContentModeration</c> <see cref="HttpClient"/> registered in DI.
/// </summary>
public sealed class ContentModerationService : IContentModerationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentModerationService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Factory used to create the named HTTP client.</param>
    /// <param name="rateLimiter">Adaptive rate limiter for API call concurrency.</param>
    /// <param name="options">Moderation configuration options.</param>
    public ContentModerationService(IHttpClientFactory httpClientFactory, ContentModerationRateLimiter rateLimiter, IOptions<ContentModerationOptions> options)
    {
        var moderationOptions = options.Value;

        _httpClient = httpClientFactory.CreateClient(HttpClientName);
        _httpClient.BaseAddress = new Uri(EnsureTrailingSlash(moderationOptions.Endpoint), UriKind.Absolute);

        if (!string.IsNullOrWhiteSpace(moderationOptions.ApiKey))
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", moderationOptions.ApiKey);

        _model = moderationOptions.Model;
        _rateLimiter = rateLimiter;
    }


    /// <inheritdoc />
    public async Task<ModerationResult> ModerateImage(byte[] imageBytes, string contentType, CancellationToken cancellationToken)
    {
        var dataUri = BuildDataUri(imageBytes, contentType);

        var body = new ModerationRequest
        {
            Model = _model,
            Input = new[]
            {
                new ModerationImageInput
                {
                    Type = "image_url",
                    ImageUrl = new ModerationImageUrl 
                    { 
                        Url = dataUri 
                    }
                }
            }
        };

        return await SendRequest(body, cancellationToken);
    }


    /// <inheritdoc />
    public async Task<ModerationResult> ModerateText(string plainText, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return ModerationResult.CreateCompleted(isFlagged: false, categoryScores: null);

        var body = new ModerationRequest
        {
            Model = _model,
            Input = plainText
        };

        return await SendRequest(body, cancellationToken);
    }


    private static string BuildDataUri(byte[] imageBytes, string contentType)
    {
        var base64 = Convert.ToBase64String(imageBytes);
        return $"data:{contentType};base64,{base64}";
    }


    private static string EnsureTrailingSlash(string endpoint)
        => endpoint.EndsWith('/') ? endpoint : endpoint + "/";


    private async Task<ModerationResult> SendRequest(object body, CancellationToken cancellationToken)
    {
        using var _ = await _rateLimiter.Acquire(cancellationToken);

        using var response = await _httpClient.PostAsJsonAsync(ModerationPath, body, _serializerOptions, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(30);
            _rateLimiter.RecordRetryAfter(retryAfter);
            _rateLimiter.RecordFailure(isRateLimitError: true);

            return ModerationResult.CreateFailed();
        }

        if (!response.IsSuccessStatusCode)
        {
            _rateLimiter.RecordFailure(isRateLimitError: false);
            return ModerationResult.CreateFailed();
        }

        var apiResponse = await response.Content.ReadFromJsonAsync<ModerationApiResponse>(_serializerOptions, cancellationToken);

        if (apiResponse?.Results is not { Length: > 0 } results)
        {
            _rateLimiter.RecordFailure(isRateLimitError: false);
            return ModerationResult.CreateFailed();
        }

        _rateLimiter.RecordSuccess();
        var first = results[0];
        
        return ModerationResult.CreateCompleted(first.Flagged, first.CategoryScores);
    }


    /// <summary>The named HTTP client key used when registering this service's client in DI.</summary>
    public const string HttpClientName = "ContentModeration";

    private const string ModerationPath = "moderations";


    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ContentModerationRateLimiter _rateLimiter;
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };


    private sealed class ModerationApiResponse
    {
        public ModerationApiResult[] Results { get; set; } = [];
    }


    private sealed class ModerationApiResult
    {
        public bool Flagged { get; set; }
        public Dictionary<string, double>? CategoryScores { get; set; }
    }


    private sealed class ModerationImageInput
    {
        public string Type { get; set; } = string.Empty;
        public ModerationImageUrl ImageUrl { get; set; } = new();
    }


    private sealed class ModerationImageUrl
    {
        public string Url { get; set; } = string.Empty;
    }


    private sealed class ModerationRequest
    {
        public string Model { get; set; } = string.Empty;
        public object Input { get; set; } = string.Empty;
    }
}
