using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;
using System.Globalization;
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
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;
using Warp.WebApp.Services.Infrastructure;
using Warp.WebApp.Services.OpenGraph;
using Warp.WebApp.Telemetry;
using Warp.WebApp.Telemetry.Logging;


var builder = WebApplication.CreateBuilder(args);

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
        .AddCheck<ControllerResolveHealthCheck>(nameof(ControllerResolveHealthCheck))
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
        return;
    }

    var secrets = VaultHelper.GetSecrets<ProgramSecrets>(logger, builder.Configuration);
    builder.AddConsulConfiguration(secrets.ConsulAddress, secrets.ConsulToken);
}


void AddOptions(ILogger<Program> logger, IServiceCollection services, IConfiguration configuration)
{
    try
    {
        services.Configure<AnalyticsOptions>(configuration.GetSection(nameof(AnalyticsOptions)));

        services.AddOptions<S3Options>()
            .Bind(configuration.GetSection(nameof(S3Options)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var imageUploadOptionsSection = configuration.GetSection("ImageUploadOptions");
        foreach (var child in imageUploadOptionsSection.GetChildren())
        {
            logger.LogInformation("Key: {Key}, Value: {Value}", child.Path, child.Value);
        }

        services.AddOptions<ImageUploadOptions>()
            .Bind(configuration.GetSection(nameof(ImageUploadOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
    catch (OptionsValidationException ex)
    {
        logger.LogOptionsValidationException(ex.Message);
        throw;
    }
}


void AddServices(IServiceCollection services)
{
    services.AddSingleton(services);

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

    services.AddHostedService<WarmupService>();
}
