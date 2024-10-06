using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using System.Globalization;
using Warp.WebApp.Data;
using Warp.WebApp.Data.Redis;
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
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Warp.WebApp.Data.S3;


var builder = WebApplication.CreateBuilder(args);

var logger = GetProgramLogger(builder);

AddConfiguration(logger, builder);

AddLogging(builder);
AddTelemetry(builder);

builder.Services.AddSingleton(_ => DistributedCacheHelper.GetConnectionMultiplexer(logger, builder.Configuration));

AddOptions(builder.Services, builder.Configuration);
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
return;


void AddOptions(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<AnalyticsOptions>(configuration.GetSection(nameof(AnalyticsOptions)));
}


void AddServices(IServiceCollection services)
{
    services.AddSingleton(services);

    services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
    services.AddTransient<IPartialViewRenderService, PartialViewRenderService>();
    services.AddTransient<IUrlService, UrlService>();
    services.AddTransient<IOpenGraphService, OpenGraphService>();

    services.AddSingleton<IImageService, ImageService>();
    services.AddSingleton<IDistributedStorage, KeyDbStorage>();
    services.AddSingleton<IDataStorage, DataStorage>();
    services.AddSingleton<IFileStorage, FileStorage>();
    services.AddTransient<IReportService, ReportService>();
    services.AddTransient<IViewCountService, ViewCountService>();
    services.AddTransient<IEntryService, EntryService>();
    services.AddTransient<ICreatorService, CreatorService>();
    services.AddTransient<ICookieService, CookieService>();

    services.AddTransient<IEntryPresentationService, EntryPresentationService>();

    services.AddHostedService<WarmupService>();
}


void AddConfiguration(ILogger<Program> logger1, WebApplicationBuilder builder1)
{
    if (builder.Environment.IsLocal())
    {
        builder1.Configuration.AddJsonFile($"appsettings.{builder1.Configuration["ASPNETCORE_ENVIRONMENT"]}.json", optional: true, reloadOnChange: true);
        return;
    }

    var secrets = VaultHelper.GetSecrets<ProgramSecrets>(logger1, builder1.Configuration);
    builder1.AddConsulConfiguration(secrets.ConsulAddress, secrets.ConsulToken);
}


void AddLogging(WebApplicationBuilder webApplicationBuilder)
{
    webApplicationBuilder.Logging.ClearProviders();

#if DEBUG
    builder.Logging.AddDebug();
#endif

    webApplicationBuilder.Logging.AddConsole();
    if (!string.IsNullOrWhiteSpace(webApplicationBuilder.Configuration["SentryDsn"]))
        webApplicationBuilder.Logging.AddSentry(o =>
        {
            o.Dsn = webApplicationBuilder.Configuration["SentryDsn"];
            o.AttachStacktrace = true;
        });

    webApplicationBuilder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
    });
}


void AddTelemetry(WebApplicationBuilder webApplicationBuilder)
{
    var openTelemetryEndpoint = webApplicationBuilder.Configuration["OpenTelemetry:Endpoint"]!;
    if (string.IsNullOrWhiteSpace(openTelemetryEndpoint))
        return;

    var serviceName = webApplicationBuilder.Configuration["ServiceName"]!;
    var openTelemetryBuilder = builder.Services.AddOpenTelemetry();

    openTelemetryBuilder.WithMetrics(metrics =>
    {
        metrics.ConfigureResource(configure => configure.AddService(serviceName))
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddMeter("Microsoft.AspNetCore.Hosting")
            .AddMeter("Microsoft.AspNetCore.Server.Kestrel");
    });

    openTelemetryBuilder.WithTracing(tracing =>
    {
        tracing.ConfigureResource(configure => configure.AddService(serviceName))
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddRedisInstrumentation()
            .SetSampler(new AlwaysOnSampler());
    });

    openTelemetryBuilder.UseOtlpExporter(OtlpExportProtocol.Grpc, new Uri(openTelemetryEndpoint));
}


ILogger<Program> GetProgramLogger(WebApplicationBuilder webApplicationBuilder)
{
    var logLevel = Enum.Parse<LogLevel>(webApplicationBuilder.Configuration["Logging:LogLevel:Default"]!);
    using var loggerFactory = LoggerFactory.Create(loggerBuilder => loggerBuilder
        .SetMinimumLevel(logLevel)
        .AddConsole());

    return loggerFactory.CreateLogger<Program>();
}
