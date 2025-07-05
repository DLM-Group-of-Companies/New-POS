using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NLI_POS.Data;
using NLI_POS.Models;
using NLI_POS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("NLPOSLiveConn") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//var connectionString = builder.Configuration.GetConnectionString("NLPOSTestConn") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseMySql(connectionString));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI(); 

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
            new PaymentMethods { Name = "Cash", Description = "Cash Payment" },
            new PaymentMethods { Name = "Credit Card", Description = "Credit Card Payment" },
            new PaymentMethods { Name = "GCash", Description = "GCash Mobile Payment" },
            new PaymentMethods { Name = "Check", Description = "Check Payment" },
            new PaymentMethods { Name = "Bank Transfer", Description = "Bank Transfer Payment" },
            new PaymentMethods { Name = "Voucher", Description = "Voucher Payment" },
            new PaymentMethods { Name = "Other", Description = "Payment not indicated on the list" }
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
