using Warp.WebApp.Data;
using Warp.WebApp.Data.Redis;
using Warp.WebApp.Helpers.Configuration;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services;

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
builder.Logging.AddSentry(o =>
{
    o.Dsn = builder.Configuration["SentryDsn"];
    o.AttachStacktrace = true;
});

builder.Services.AddSingleton(_ => DistributedCacheHelper.GetConnectionMultiplexer(logger, builder.Configuration));

builder.Services.AddSingleton<IImageService, ImageService>();
builder.Services.AddSingleton<IDistributedStorage, KeyDbStorage>();
builder.Services.AddSingleton<IDataStorage, DataStorage>();
builder.Services.AddTransient<IReportService, ReportService>();
builder.Services.AddTransient<IViewCountService, ViewCountService>();
builder.Services.AddTransient<IEntryService, EntryService>();

builder.Services.AddMemoryCache();
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

/*app.UseWhen(context => IsApiRequest(context.Request), appBuilder =>
{
    appBuilder.ConfigureApiExceptionHandler(app.Environment);
});

app.UseWhen(context => !IsApiRequest(context.Request), appBuilder =>
{
    appBuilder.UseExceptionHandler("/Error");
});*/

if (!app.Environment.IsDevelopmentOrLocal())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseHealthChecks("/health");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();
return;


ILogger<Program> GetProgramLogger(WebApplicationBuilder webApplicationBuilder)
{
    var logLevel = Enum.Parse<LogLevel>(webApplicationBuilder.Configuration["Logging:LogLevel:Default"]!);
    using var loggerFactory = LoggerFactory.Create(loggerBuilder => loggerBuilder
        .SetMinimumLevel(logLevel)
        .AddConsole());

    return loggerFactory.CreateLogger<Program>();
}


static bool IsApiRequest(HttpRequest request)
    => request.Path.StartsWithSegments(new PathString("/api"), StringComparison.InvariantCultureIgnoreCase);