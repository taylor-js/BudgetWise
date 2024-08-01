using BudgetWise.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using BudgetWise.Data;
using Microsoft.AspNetCore.Identity;
using BudgetWise.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();

var connectionString = builder.Configuration.GetConnectionString("HerokuPostgres");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireUppercase = false;
});

// Configure cookie policy to handle SameSite attribute
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.Always; // Ensure cookies are sent over HTTPS
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = opt =>
    {
        opt.HttpContext.Response.Redirect("/Identity/Account/Login");
        return Task.FromResult(0);
    };
});

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2UFhhQlJBfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTX5WdkNjXH5edHxUR2VU");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Handle exceptions and redirect to Error page
    app.UseHsts(); // Add HTTP Strict Transport Security (HSTS) headers for added security
}

app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // Ensure authentication middleware is added
app.UseAuthorization();
app.UseCookiePolicy(); // Apply the cookie policy

// Middleware to handle domain redirection
app.Use(async (context, next) =>
{
    var request = context.Request;
    var host = request.Host.ToString();

    // Redirect from Heroku domain to custom domain
    if (host == "budgetwise-expense-tracker-f4aae4b8ebbc.herokuapp.com")
    {
        var newUrl = $"budget-wise.net{request.Path}{request.QueryString}";
        context.Response.Redirect(newUrl);
    }
    else
    {
        await next();
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=DemoDashboard}/{action=Demo}/{id?}");

app.MapControllerRoute(
    name: "dashboard",
    pattern: "Dashboard/{action=Index}/{id?}",
    defaults: new { controller = "Dashboard" });

app.MapControllerRoute(
    name: "category",
    pattern: "Category/{action=Index}/{id?}",
    defaults: new { controller = "Category" });

app.MapControllerRoute(
    name: "transaction",
    pattern: "Transaction/{action=Index}/{id?}",
    defaults: new { controller = "Transaction" });

app.MapRazorPages();

app.Run();
