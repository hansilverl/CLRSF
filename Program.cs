using CLSF_Compare.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register the exchange rate service
builder.Services.AddScoped<IExchangeRateService, BOIExchangeRateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Calculator}/{action=ManualInput}/{id?}");

});

app.Run();