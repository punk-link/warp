using Warp.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IImageService, ImageService>();
builder.Services.AddSingleton<IReportStorage, ReportStorage>();
builder.Services.AddTransient<IReportService, ReportService>();
builder.Services.AddTransient<IViewCountService, ViewCountService>();
builder.Services.AddTransient<IWarpContentService, WarpContentService>();

builder.Services.AddMemoryCache();
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
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
