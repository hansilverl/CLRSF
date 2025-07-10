using CurrencyComparisonTool.Services;
using System.Text; 

var builder = WebApplication.CreateBuilder(args);

// Register encoding provider (for PdfSharp/MigraDoc)
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IExchangeRateService, BOIExchangeRateService>();
builder.Services.AddScoped<IExportService, PdfExportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
