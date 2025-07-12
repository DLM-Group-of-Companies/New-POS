using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Services;
using System.Net.Http;
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var localTimeZone = "UTC";

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("NLPOSLiveConn") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//var connectionString = builder.Configuration.GetConnectionString("NLPOSTestConn") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseMySql(connectionString));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

//builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddSession();

// Add ToastNotification
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 5;
    config.IsDismissable = true;
    config.Position = NotyfPosition.BottomRight;
});

builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

builder.Services.AddHttpContextAccessor();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnSignedIn = async context =>
    {
        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
        var user = await userManager.GetUserAsync(context.Principal);

        var ipAddress = context.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                        ?? context.HttpContext.Connection.RemoteIpAddress?.ToString();
        
        // Define a typed HttpClient (or use IHttpClientFactory ideally)
        using var httpClient = new HttpClient();
        GeoResponse? geoData = null;

        try
        {
            var response = await httpClient.GetStringAsync($"http://ip-api.com/json/{ipAddress}");
            geoData = JsonSerializer.Deserialize<GeoResponse>(response);
            localTimeZone = geoData?.Timezone ?? "UTC";
            var session = context.HttpContext.Session;
            session.SetString("localTimeZone", localTimeZone);
            session.SetString("IP", ipAddress);
        }
        catch
        {
            // handle error (API down, rate limited, etc.)
        }

        db.AuditLogs.Add(new AuditLog
        {
            UserId = user?.Id,
            Activity = "Logged in",
            Timestamp = AuditHelpers.GetLocalTime(localTimeZone),
            Path = context.Request.Path,
            IP = ipAddress
        });

        await db.SaveChangesAsync();
    };
});



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    //Seeding For User Roles
    var services = scope.ServiceProvider;
    await IdentitySeeder.SeedRolesAsync(services);

    //Seeding For Payment Methods
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.PaymentMethods.Any())
    {
        context.PaymentMethods.AddRange(
            new PaymentMethod { Name = "Cash", Description = "Cash Payment" },
            new PaymentMethod { Name = "Credit Card", Description = "Credit Card Payment" },
            new PaymentMethod { Name = "GCash", Description = "GCash Mobile Payment" },
            new PaymentMethod { Name = "Check", Description = "Check Payment" },
            new PaymentMethod { Name = "Bank Transfer", Description = "Bank Transfer Payment" },
            new PaymentMethod { Name = "Voucher", Description = "Voucher Payment" },
            new PaymentMethod { Name = "Other", Description = "Payment not indicated on the list" }
        );
        context.SaveChanges();
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.UseNotyf();

app.MapRazorPages();

app.Run();
