using Microsoft.AspNetCore.ResponseCompression;
using Warp.WebApp.Data;
using Warp.WebApp.Data.Redis;
using Warp.WebApp.Helpers.Configuration;
using Warp.WebApp.Helpers.HealthChecks;
using Warp.WebApp.Helpers.Warmups;
using Warp.WebApp.Middlewares;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services.Entries;
using Warp.WebApp.Services.Images;

var builder = WebApplication.CreateBuilder(args);

var logger = GetProgramLogger(builder);

var secrets = VaultHelper.GetSecrets<ProgramSecrets>(logger, builder.Configuration);

builder.AddConsulConfiguration(secrets.ConsulAddress, secrets.ConsulToken);
// Restores local setting for development purposes (e.g. local port forwarding)
builder.Configuration.AddJsonFile($"appsettings.{builder.Configuration["ASPNETCORE_ENVIRONMENT"]}.json", optional: true, reloadOnChange: true);

builder.Logging.ClearProviders();

#if DEBUG
builder.Logging.AddDebug();
#endif

builder.Logging.AddConsole();
if (!string.IsNullOrWhiteSpace(builder.Configuration["SentryDsn"]))
    builder.Logging.AddSentry(o =>
    {
        o.Dsn = builder.Configuration["SentryDsn"];
        o.AttachStacktrace = true;
    });


builder.Services.AddSingleton(_ => DistributedCacheHelper.GetConnectionMultiplexer(logger, builder.Configuration));

AddServices(builder.Services);

builder.Services.AddMemoryCache();
builder.Services.AddRazorPages();
builder.Services.AddControllers()
    .AddControllersAsServices();
builder.Services.AddHealthChecks()
    .AddCheck<ControllerResolveHealthCheck>(nameof(ControllerResolveHealthCheck));

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddResponseCaching();
builder.Services.AddOutputCache();

var app = builder.Build();

if (!app.Environment.IsDevelopmentOrLocal())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseResponseCaching();
app.UseOutputCache();

app.UseHealthChecks("/health");
app.UseMiddleware<RobotsMiddleware>();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
return;


IServiceCollection AddServices(IServiceCollection services)
{
    services.AddSingleton(services);

    services.AddSingleton<IImageService, ImageService>();
    services.AddSingleton<IDistributedStorage, KeyDbStorage>();
    services.AddSingleton<IDataStorage, DataStorage>();
    services.AddTransient<IReportService, ReportService>();
    services.AddTransient<IViewCountService, ViewCountService>();
    services.AddTransient<IEntryService, EntryService>();

    services.AddHostedService<WarmupService>();

    return services;
}


ILogger<Program> GetProgramLogger(WebApplicationBuilder webApplicationBuilder)
{
    var logLevel = Enum.Parse<LogLevel>(webApplicationBuilder.Configuration["Logging:LogLevel:Default"]!);
    using var loggerFactory = LoggerFactory.Create(loggerBuilder => loggerBuilder
        .SetMinimumLevel(logLevel)
        .AddConsole());

    return loggerFactory.CreateLogger<Program>();
}
