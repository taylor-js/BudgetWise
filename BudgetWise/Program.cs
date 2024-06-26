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
        //opt.HttpContext.Response.Redirect("/DemoDashboard/Demo");
        return Task.FromResult(0);
    };
});

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2UFhhQlJBfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTX5WdkNjXH5edHxUR2VU");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Handle exceptions and redirect to Error page
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // Add this line to ensure authentication middleware is added
app.UseAuthorization();
app.UseCookiePolicy(); // Add this line to apply the cookie policy

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

// Configure routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=DemoDashboard}/{action=Demo}/{id?}");

app.MapGet("/", context =>
{
    context.Response.Redirect("/DemoDashboard/Demo", permanent: true);
    return Task.CompletedTask;
});

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
