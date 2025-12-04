using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.ResponseCompression;
using System.Text.Json;
using System.Text.Json.Serialization;
using Warp.WebApp.Constants;
using Warp.WebApp.Data;
using Warp.WebApp.Data.Redis;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Filters;
using Warp.WebApp.Helpers.Configuration;
using Warp.WebApp.Helpers.HealthChecks;
using Warp.WebApp.Helpers.Warmups;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Encryption;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.OpenGraph;
using Warp.WebApp.Telemetry.Logging;
using Warp.WebApp.Telemetry.Metrics;

namespace Warp.WebApp.Extensions;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(services);

        services.AddTransient(_ => VaultHelper.GetVaultClient(configuration));
    
        services.AddTransient<IDistributedStore, KeyDbStore>();
        services.AddTransient<IS3FileStorage, S3FileStorage>();
        services.AddSingleton<IEntryLifecycleIndexStore, EntryLifecycleIndexStore>();

        if (configuration["EncryptionOptions:Type"] == "AesEncryptionService")
            services.AddTransient<IEncryptionService, AesEncryptionService>();
        else
            services.AddSingleton<IEncryptionService, TransitEncryptionService>();

        services.AddTransient<IOpenGraphService, OpenGraphService>();

        services.AddTransient<IUnauthorizedImageService, ImageService>();
        services.AddTransient<IImageService, ImageService>();
        services.AddScoped<IEntryImageLifecycleService, EntryImageLifecycleService>();
        services.AddTransient<IDataStorage, DataStorage>();
        services.AddTransient<IAmazonS3Factory, AmazonS3Factory>();
        services.AddTransient<IReportService, ReportService>();
        services.AddTransient<IViewCountService, ViewCountService>();
        services.AddTransient<IEntryInfoService, EntryInfoService>();
        services.AddTransient<IEntryService, EntryService>();
        services.AddTransient<ICreatorService, CreatorService>();
        services.AddTransient<ICookieService, CookieService>();
        services.AddSingleton<IEntryInfoMetrics, EntryInfoMetrics>();
    
        services.AddSingleton<IRouteWarmer, RouteWarmerService>();
        services.AddSingleton<IServiceWarmer, ServiceWarmerService>();

        services.AddScoped<RequireCreatorCookieFilter>();
        services.AddScoped<ValidateIdFilter>();

        services.AddHostedService<WarmupService>();
        services.AddHostedService<EntryImageCleanupService>();
        services.AddHostedService<OrphanImageCleanupService>();

        return services;
    }


    internal static IServiceCollection AddOptions(this IServiceCollection services, WebApplicationBuilder builder, ILogger logger)
    {
        try
        {
            services.AddOptions<RoutesWarmupOptions>()
                .BindConfiguration(nameof(RoutesWarmupOptions));

            services.AddOptions<AnalyticsOptions>()
                .BindConfiguration(nameof(AnalyticsOptions));

            services.AddOptions<S3Options>()
                .Configure(options =>
                {
                    options.AccessKey = builder.Configuration["S3Options:AccessKey"]!;
                    options.BucketName = builder.Configuration["S3Options:BucketName"]!;
                    options.SecretAccessKey = builder.Configuration["S3Options:SecretAccessKey"]!;
                })
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<ImageUploadOptions>()
                .Configure(options =>
                {
                    // AppSettings configuration provider returns an array of string,
                    // but Consul configuration provider returns a single string.
                    // So we need to handle both cases differently.
                    var allowedExtensionsSection = builder.Configuration.GetSection("ImageUploadOptions:AllowedExtensions");
                    var allowedExtensions = Array.Empty<string>();

                    if (allowedExtensionsSection.Value is not null)
                        allowedExtensions = JsonSerializer.Deserialize<string[]>(allowedExtensionsSection.Value)!;
                    else if (allowedExtensionsSection.GetChildren().Any())
                        allowedExtensions = allowedExtensionsSection.Get<string[]>() ?? [];

                    options.AllowedExtensions = allowedExtensions;
                    options.MaxFileCount = builder.Configuration.GetValue<int>("ImageUploadOptions:MaxFileCount");
                    options.MaxFileSize = builder.Configuration.GetValue<long>("ImageUploadOptions:MaxFileSize");
                    options.RequestBoundaryLengthLimit = builder.Configuration.GetValue<int>("ImageUploadOptions:RequestBoundaryLengthLimit");
                })
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<EntryCleanupOptions>()
                .BindConfiguration(nameof(EntryCleanupOptions))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<OrphanImageCleanupOptions>()
                .BindConfiguration(nameof(OrphanImageCleanupOptions))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            if (builder.Environment.IsLocal() || builder.Environment.IsEndToEndTests())
            {
                services.AddOptions<EncryptionOptions>()
                    .Configure(options =>
                    {
                        if (builder.Configuration["EncryptionOptions:Type"] == "AesEncryptionService")
                        { 
                            string base64EncryptionKey;

                            logger.LogInformation("Using AesEncryptionService encryption configuration");
                            var encryptionKeyPath = builder.Configuration["EncryptionOptions:KeyFilePath"];
                            if (!string.IsNullOrEmpty(encryptionKeyPath) && File.Exists(encryptionKeyPath))
                            {
                                logger.LogInformation($"Using encryption key from file: {encryptionKeyPath}");
                                base64EncryptionKey = File.ReadAllText(encryptionKeyPath).Trim();
                            }
                            else
                            {
                                logger.LogWarning($"Encryption key file not found at {encryptionKeyPath} or not specified. Using encryption key from WARP_ENCRYPTION_KEY environment variable");
                                var envKey = Environment.GetEnvironmentVariable("WARP_ENCRYPTION_KEY");
                                if (string.IsNullOrEmpty(envKey))
                                    throw new Exception("Encryption key not found in any source. Please specify it in the configuration or as an environment variable.");

                                base64EncryptionKey = envKey ?? string.Empty;
                            }

                            var encryptionKey = Convert.FromBase64String(base64EncryptionKey);
                            if (encryptionKey.Length != 32) // 256 bits = 32 bytes
                                throw new Exception($"Encryption key length is not 32 bytes (256 bits). Key length: {encryptionKey.Length}");

                            options.EncryptionKey = encryptionKey;
                            options.TransitKeyName = null;

                            return;
                        }
                    
                        logger.LogInformation("Using TransitEncryptionService encryption configuration");
                    
                        options.TransitKeyName = builder.Configuration["EncryptionOptions:TransitKeyName"];
                        options.EncryptionKey = null;
                    })
                .ValidateDataAnnotations()
                .ValidateOnStart();
            }

            services.AddOptions<OpenGraphOptions>()
                .Configure(options =>
                {
                    options.DefaultImageUrl = new Uri(builder.Configuration["OpenGraph:DefaultImageUrl"]!);
                    options.Title = builder.Configuration["OpenGraph:Title"]!;
                });
        }
        catch (Exception ex)
        {
            logger.LogOptionsValidationException(ex.Message);
            throw;
        }

        return services;
    }


    internal static IServiceCollection AddServices(this IServiceCollection services, WebApplicationBuilder builder, ILogger logger)
    {
        services.AddSingleton(_ => DistributedCacheHelper.GetConnectionMultiplexer(logger, builder.Configuration));
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = DistributedCacheHelper.GetConnectionString(logger, builder.Configuration);
            options.InstanceName = $"WarpCache::{builder.Environment}::";
        });

        services.AddOptions(builder, logger)
            .AddApplicationDependencies(builder.Configuration);

        services.AddLocalization(o => o.ResourcesPath = "Resources");

        services.AddMemoryCache();
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .AddControllersAsServices();

        services.AddHealthChecks()
            .AddCheck<RedisHealthCheck>(nameof(RedisHealthCheck));

        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        services.AddResponseCaching();
        services.AddOutputCache();

        var allowInsecureCookies = InsecureCookiesHelper.IsAllowed(builder.Environment);
        var securePolicy = allowInsecureCookies ? CookieSecurePolicy.None : CookieSecurePolicy.Always;

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.SecurePolicy = securePolicy;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Name = "Warp.Auth";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(365);
                options.SlidingExpiration = true;
            });

        // Antiforgery for SPA + API (same-origin)
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.FormFieldName = "__RequestVerificationToken";
            options.Cookie.Name = "Warp.AntiForgery";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = securePolicy;
            options.Cookie.HttpOnly = true;
        });

        services.AddHttpClient(HttpClients.Warmup);

        return services;
    }
}
