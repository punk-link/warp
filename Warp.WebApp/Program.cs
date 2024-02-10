using Warp.WebApp.Data;
using Warp.WebApp.Data.Redis;
using Warp.WebApp.Helpers.Configuration;
using Warp.WebApp.Models.Options;
using Warp.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

var logLevel = Enum.Parse<LogLevel>(builder.Configuration["Logging:LogLevel:Default"]!);
using var loggerFactory = LoggerFactory.Create(loggerBuilder => loggerBuilder
    .SetMinimumLevel(logLevel)
    .AddConsole());

var logger = loggerFactory.CreateLogger<Program>();

var secrets = VaultHelper.GetSecrets<ProgramSecrets>(logger, builder.Configuration);
builder.AddConsulConfiguration(secrets.ConsulAddress, secrets.ConsulToken);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSingleton(_ => DistributedCacheHelper.GetConnectionMultiplexer(logger, builder.Configuration));

builder.Services.AddSingleton<IImageService, ImageService>();
builder.Services.AddSingleton<IReportStorage, ReportStorage>();
builder.Services.AddSingleton<IDistributedStorage, RedisStorage>();
builder.Services.AddSingleton<IDataStorage, DataStorage>();
builder.Services.AddTransient<IReportService, ReportService>();
builder.Services.AddTransient<IViewCountService, ViewCountService>();
builder.Services.AddTransient<IWarpContentService, WarpContentService>();

builder.Services.AddMemoryCache();
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment() || !app.Environment.IsEnvironment("Local"))
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