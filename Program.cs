using CLSF_Compare.Services;
using Microsoft.Extensions.FileProviders;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the exchange rate service
builder.Services.AddScoped<IExchangeRateService, BOIExchangeRateService>();

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Content")),
    RequestPath = "/Content"
});


// Configure the HTTP request pipeline.
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Calculator}/{action=ManualInput}/{id?}");

app.Run();  
