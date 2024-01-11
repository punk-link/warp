using Warp.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IWarpContentService, WarpContentService>();

builder.Services.AddMemoryCache();
builder.Services.AddRazorPages();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseHealthChecks("/health");

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
