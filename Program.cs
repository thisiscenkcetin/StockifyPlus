using StockifyPlus.Data;
using StockifyPlus.Extensions;
using StockifyPlus.Hubs;
using StockifyPlus.Middleware;
using StockifyPlus.Models;
using StockifyPlus.Models.Enums;
using StockifyPlus.Services.Background;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using DotNetEnv;

try
{
    Env.Load();
}
catch
{
}

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("BaÄŸlantÄ± dizesi yapÄ±landÄ±rÄ±lmadÄ±.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddHostedService<StockAlertBackgroundService>();

builder.Services.AddControllersWithViews();

var trCulture = new CultureInfo("tr-TR");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(trCulture);
    options.SupportedCultures = new[] { trCulture };
    options.SupportedUICultures = new[] { trCulture };
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        await EnsureDemoAdminUserAsync(context);
        await NormalizeLegacyPriceScaleAsync(context, app.Environment.ContentRootPath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Startup Warning] VeritabanÄ± migration uygulanamadÄ±: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandlingMiddleware();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var localizationOptions = app.Services
    .GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>()
    .Value;
app.UseRequestLocalization(localizationOptions);

app.UseRouting();
app.UseSession();

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    var isAccountRoute = path.StartsWithSegments("/Account/Login", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/Account/Register", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/Account/Logout", StringComparison.OrdinalIgnoreCase);

    var isAllowedPublicRoute = path.StartsWithSegments("/favicon.ico", StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments("/hubs/notification", StringComparison.OrdinalIgnoreCase);

    if (isAccountRoute || isAllowedPublicRoute)
    {
        await next();
        return;
    }

    var userId = context.Session.GetString("UserId");
    if (string.IsNullOrWhiteSpace(userId))
    {
        context.Response.Redirect("/Account/Login");
        return;
    }

    await next();
});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapHub<NotificationHub>("/hubs/notification");

app.Run();

static async Task EnsureDemoAdminUserAsync(ApplicationDbContext context)
{
    const string demoUsername = "admin";
    const string demoPassword = "Admin123!";
    const string demoEmail = "admin@stockifyplus.local";

    var demoUser = await context.AppUsers.FirstOrDefaultAsync(u => u.Username == demoUsername);
    var demoHash = HashPassword(demoPassword);

    if (demoUser == null)
    {
        demoUser = new AppUser
        {
            Username = demoUsername,
            PasswordHash = demoHash,
            FullName = "Demo Admin",
            Email = demoEmail,
            Role = UserRole.Admin,
            IsActive = true,
            CreatedDate = DateTime.Now
        };

        await context.AppUsers.AddAsync(demoUser);
        await context.SaveChangesAsync();
        return;
    }

    demoUser.PasswordHash = demoHash;
    demoUser.Role = UserRole.Admin;
    demoUser.IsActive = true;
    if (string.IsNullOrWhiteSpace(demoUser.Email))
    {
        demoUser.Email = demoEmail;
    }

    context.AppUsers.Update(demoUser);
    await context.SaveChangesAsync();
}

static string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
    return Convert.ToBase64String(hashedBytes);
}

static async Task NormalizeLegacyPriceScaleAsync(ApplicationDbContext context, string contentRootPath)
{
    var markerPath = Path.Combine(contentRootPath, ".price-normalization-v1.done");
    if (File.Exists(markerPath))
    {
        return;
    }

    var candidates = await context.Products
        .Where(p => p.Price >= 1000000m && p.Price <= 100000000m)
        .ToListAsync();

    if (candidates.Count > 0)
    {
        foreach (var product in candidates)
        {
            product.Price = decimal.Round(product.Price / 100m, 2);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[Startup Info] Legacy fiyat duzeltmesi uygulandi. Guncellenen urun: {candidates.Count}");
    }

    File.WriteAllText(markerPath, $"AppliedAt={DateTime.UtcNow:O}");
}
