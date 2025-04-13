using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.FeatureManagement;
using System.Globalization;
using System.Text.Json;
using Warp.WebApp.Constants;
using Warp.WebApp.Data;
using Warp.WebApp.Data.Redis;
using Warp.WebApp.Data.S3;
using Warp.WebApp.Helpers.Configuration;
using Warp.WebApp.Helpers.HealthChecks;
using Warp.WebApp.Helpers.Warmups;
using Warp.WebApp.Middlewares;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services;
using Warp.WebApp.Services.Creators;
using Warp.WebApp.Services.Encryption;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.Infrastructure;
using Warp.WebApp.Services.OpenGraph;
using Warp.WebApp.Telemetry;
using Warp.WebApp.Telemetry.Logging;


var builder = WebApplication.CreateBuilder(args);
builder.Host.UseDefaultServiceProvider((context, options) =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

var startupLogger = builder.GetStartUpLogger();

try
{
    AddConfiguration(startupLogger, builder);

    builder.AddLogging()
        .AddTelemetry();

    builder.Services.AddSingleton(_ => DistributedCacheHelper.GetConnectionMultiplexer(startupLogger, builder.Configuration));

    AddOptions(startupLogger, builder.Services, builder.Configuration);
    AddServices(builder.Services);

    builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

    builder.Services.AddMemoryCache();
    builder.Services.AddRazorPages()
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
        .AddDataAnnotationsLocalization();
    builder.Services.AddControllers()
        .AddControllersAsServices();
    builder.Services.AddHealthChecks()
        // TODO: remove
        //.AddCheck<ControllerResolveHealthCheck>(nameof(ControllerResolveHealthCheck)) 
        .AddCheck<RedisHealthCheck>(nameof(RedisHealthCheck));

    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
    });

    builder.Services.AddResponseCaching();
    builder.Services.AddOutputCache();

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

    builder.Services.AddHttpClient(HttpClients.Warmup);

    var app = builder.Build();

    var supportedCultures = new[] { new CultureInfo("en-US") };
    app.UseRequestLocalization(new RequestLocalizationOptions
    {
        DefaultRequestCulture = new RequestCulture("en-US"),
        SupportedCultures = supportedCultures,
        SupportedUICultures = supportedCultures
    });

    if (!app.Environment.IsDevelopmentOrLocal())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseMiddleware<ApiExceptionHandlerMiddleware>();
    app.UseMiddleware<TraceMethodMiddleware>();

    app.UseHttpsRedirection();
    app.UseResponseCompression();
    app.UseResponseCaching();
    app.UseOutputCache();

    app.UseHealthChecks("/health");
    app.UseMiddleware<CancellationExceptionHandlerMiddleware>();
    app.UseMiddleware<RobotsMiddleware>();
    app.UseStaticFiles();
    app.UseCookiePolicy(new CookiePolicyOptions()
    {
        Secure = CookieSecurePolicy.Always,
        MinimumSameSitePolicy = SameSiteMode.Strict
    });

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapRazorPages();

    app.Run();
}
catch (Exception ex)
{
    startupLogger.LogCritical(ex, ex.Message);
    throw;
}

return;


void AddConfiguration(ILogger<Program> logger, WebApplicationBuilder builder)
{
    if (builder.Environment.IsLocal())
    {
        logger.LogLocalConfigurationIsInUse();
        builder.Configuration.AddJsonFile($"appsettings.{builder.Configuration["ASPNETCORE_ENVIRONMENT"]}.json", optional: true, reloadOnChange: true);
    }
    else
    {
        var secrets = VaultHelper.GetSecrets<ProgramSecrets>(logger, builder.Configuration);
        builder.AddConsulConfiguration(secrets.ConsulAddress, secrets.ConsulToken);
    }

    builder.Services.AddFeatureManagement();
}


void AddOptions(ILogger<Program> logger, IServiceCollection services, IConfiguration configuration)
{
    try
    {
        services.AddOptions<RoutesWarmupOptions>()
            .BindConfiguration(nameof(RoutesWarmupOptions));

        services.AddOptions<AnalyticsOptions>()
            .BindConfiguration(nameof(AnalyticsOptions));

        services.AddOptions<S3Options>()
            .BindConfiguration(nameof(S3Options))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<EncryptionOptions>()
            .Configure(options =>
            {
                if (builder.Environment.IsLocal())
                {
                    string base64EncriptionKey;

                    logger.LogInformation("Local environment detected - Using local encryption configuration");
                    var encriptionKeyPath = configuration["EncryptionOptions:KeyFilePath"];
                    if (!string.IsNullOrEmpty(encriptionKeyPath) && File.Exists(encriptionKeyPath))
                    {
                        logger.LogInformation($"Using encryption key from file: {encriptionKeyPath}");
                        base64EncriptionKey = File.ReadAllText(encriptionKeyPath).Trim();
                    }
                    else
                    {
                        logger.LogWarning($"Encryption key file not found at {encriptionKeyPath} or not specified. Using encryption key from WARP_ENCRYPTION_KEY environment variable");
                        var envKey = Environment.GetEnvironmentVariable("WARP_ENCRYPTION_KEY");
                        if (string.IsNullOrEmpty(envKey))
                            throw new Exception("Encryption key not found in any source. Please specify it in the configuration or as an environment variable.");

                        base64EncriptionKey = envKey ?? string.Empty;
                    }

                    var encryptionKey = Convert.FromBase64String(base64EncriptionKey);
                    if (encryptionKey.Length != 32) // 256 bits = 32 bytes
                        throw new Exception($"Encryption key length is not 32 bytes (256 bits). Key length: {encryptionKey.Length}");

                    options.EncryptionKey = encryptionKey;
                }
                //else
                //{
                //    // Production environment - prioritize Vault
                //    logger.LogInformation("Production environment detected - Using Vault for encryption key");
                    
                //    try
                //    {
                //        // Try to get the encryption key from Vault
                //        var vaultClient = VaultHelper.GetVaultClient(configuration);
                        
                //        // Default path: secretName is "encryption-keys" and key is "keydb"
                //        var response = vaultClient.Secrets
                //            .KvV2Read("encryption-keys", "secrets")
                //            .GetAwaiter()
                //            .GetResult();
                        
                //        if (response?.Data?.Data != null && 
                //            response.Data.Data.ContainsKey("keydb") && 
                //            response.Data.Data["keydb"] is string keyString)
                //        {
                //            options.EncryptionKey = keyString;
                //            logger.LogInformation("Successfully loaded encryption key from Vault");
                //        }
                //        else
                //        {
                //            logger.LogWarning("Encryption key not found in Vault");
                            
                //            // Still allow the fallback to configuration even in production
                //            // But with a warning since Vault should be preferred
                //            configuration.GetSection(nameof(EncryptionOptions)).Bind(options);
                //            if (!string.IsNullOrEmpty(options.KeyFilePath) || !string.IsNullOrEmpty(options.EncryptionKey))
                //            {
                //                logger.LogWarning("Using fallback encryption options from configuration in production");
                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        logger.LogError(ex, "Failed to retrieve encryption key from Vault");
                        
                //        // Fall back to configuration but log the error
                //        configuration.GetSection(nameof(EncryptionOptions)).Bind(options);
                //        if (!string.IsNullOrEmpty(options.KeyFilePath) || !string.IsNullOrEmpty(options.EncryptionKey))
                //        {
                //            logger.LogWarning("Using fallback encryption options from configuration due to Vault error");
                //        }
                //    }
                //}
            })
            .ValidateDataAnnotations();

        services.AddOptions<ImageUploadOptions>()
            .Configure(options =>
            {
                // AppSettings configuration provider returns an array of string,
                // but Consul configuration provider returns a single string.
                // So we need to handle both cases differently.
                var allowedExtensionsSection = configuration.GetSection("ImageUploadOptions:AllowedExtensions");
                var allowedExtensions = Array.Empty<string>();

                if (allowedExtensionsSection.Value is not null)
                    allowedExtensions = JsonSerializer.Deserialize<string[]>(allowedExtensionsSection.Value)!;
                else if (allowedExtensionsSection.GetChildren().Any())
                    allowedExtensions = allowedExtensionsSection.Get<string[]>() ?? [];

                options.AllowedExtensions = allowedExtensions;
                options.MaxFileCount = configuration.GetValue<int>("ImageUploadOptions:MaxFileCount");
                options.MaxFileSize = configuration.GetValue<long>("ImageUploadOptions:MaxFileSize");
                options.RequestBoundaryLengthLimit = configuration.GetValue<int>("ImageUploadOptions:RequestBoundaryLengthLimit");
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
    catch (Exception ex)
    {
        logger.LogOptionsValidationException(ex.Message);
        throw;
    }
}


void AddServices(IServiceCollection services)
{
    services.AddSingleton(services);

    services.AddSingleton<IEncryptionService, AesEncryptionService>();

    services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
    services.AddTransient<IPartialViewRenderService, PartialViewRenderService>();
    services.AddTransient<IUrlService, UrlService>();
    services.AddTransient<IOpenGraphService, OpenGraphService>();

    services.AddTransient<IUnauthorizedImageService, ImageService>();
    services.AddTransient<IImageService, ImageService>();
    services.AddTransient<IDistributedStorage, KeyDbStorage>();
    services.AddTransient<IDataStorage, DataStorage>();
    services.AddTransient<IAmazonS3Factory, AmazonS3Factory>();
    services.AddTransient<IS3FileStorage, S3FileStorage>();
    services.AddTransient<IReportService, ReportService>();
    services.AddTransient<IViewCountService, ViewCountService>();
    services.AddTransient<IEntryInfoService, EntryInfoService>();
    services.AddTransient<IEntryService, EntryService>();
    services.AddTransient<ICreatorService, CreatorService>();
    services.AddTransient<ICookieService, CookieService>();
    
    services.AddSingleton<IRouteWarmer, RouteWarmerService>();
    services.AddSingleton<IServiceWarmer, ServiceWarmerService>();
    services.AddHostedService<WarmupService>();
}
