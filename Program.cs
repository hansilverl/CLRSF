// Entry point of the application

// Initialize a new instance of the WebApplication builder class
// This class sets up the configuration services and the web server 
var builder = WebApplication.CreateBuilder(args);

// Add services to the container. Adding support for both controllers and views
builder.Services.AddControllersWithViews();

// Compiles the app, creating a web application instance
var app = builder.Build();

// Configure the HTTP request pipeline. 
// This pipeling detirmines how requests are processed by the application
if (!app.Environment.IsDevelopment())  
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();  // Redirects HTTP requests to HTTPS
app.UseStaticFiles();    // Serves static files, like HTML, CSS, images etc.

app.UseRouting();   // allows the app to route incoming requests to the appropriate endpoint (controller action)

app.UseAuthorization(); // Responsible for authorizing the user to access secured resources


// default route pattern , currently maps to the home controller and the index action method
// the id parameter is optional
app.MapControllerRoute( 
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
