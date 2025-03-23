using BudgetWise.Models;
using BudgetWise.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Identity.UI.Services;
using BudgetWise.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
ConfigureApp(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Database Configuration
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string is not configured.");
    }
    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
    services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(connectionString));

    // Identity Configuration
    services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireUppercase = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>() // Use ApplicationDbContext instead of AuthDbContext
    .AddDefaultTokenProviders();

    // MVC and Razor Pages
    services.AddControllersWithViews()
        .AddRazorRuntimeCompilation();
    services.AddRazorPages();

    // HTTP Client
    services.AddHttpClient();

    // Core Services
    services.AddMemoryCache();
    services.AddLogging();

    // Cookie Policy Configuration
    services.Configure<CookiePolicyOptions>(options =>
    {
        options.MinimumSameSitePolicy = SameSiteMode.None;
        options.Secure = CookieSecurePolicy.Always;
        options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    });

    // Identity Cookie Configuration
    services.ConfigureApplicationCookie(options =>
    {
        options.Events.OnRedirectToLogin = context =>
        {
            context.HttpContext.Response.Redirect("/Identity/Account/Login");
            return Task.CompletedTask;
        };
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Ensure HTTPS is being used
        options.Cookie.SameSite = SameSiteMode.Lax; // Change to 'Lax' if 'Strict' causes issues
    });

    // Syncfusion Configuration
    var syncfusionLicenseKey = configuration["Syncfusion:LicenseKey"];
    if (string.IsNullOrEmpty(syncfusionLicenseKey))
    {
        // Default to hardcoded key if not in configuration
        syncfusionLicenseKey = "Mgo+DSMBMAY9C3t2UFhhQlJBfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTX5WdkNjXH5edHxUR2VU";
    }
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);
}

void ConfigureApp(WebApplication app)
{
    // Development/Production environment configuration
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    // Middleware Pipeline
    // app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseCookiePolicy();

    // Middleware to handle domain redirection
    app.Use(async (context, next) =>
    {
        var request = context.Request;
        var host = request.Host.ToString();
        // Redirect from Heroku domain to custom domain
        if (host == "budgetwise-expense-tracker-f4aae4b8ebbc.herokuapp.com")
        {
            var newUrl = $"https://www.budget-wise.net{request.Path}{request.QueryString}";
            context.Response.Redirect(newUrl);
        }
        else
        {
            await next();
        }
    });

    // Route Configuration
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
}