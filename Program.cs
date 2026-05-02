using StockifyPlus.Data;
using StockifyPlus.Extensions;
using StockifyPlus.Helpers;
using StockifyPlus.Hubs;
using StockifyPlus.Middleware;
using StockifyPlus.Models;
using StockifyPlus.Models.Enums;
using StockifyPlus.Services.Background;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using DotNetEnv;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("StockifyPlus baþlatýlýyor...");

try
{
    Env.Load();
    Log.Information(".env dosyasý yüklendi");
}
catch (Exception ex)
{
    Log.Warning(ex, ".env dosyasý yüklenemedi, varsayýlan yapýlandýrma kullanýlacak");
}

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/stockifyplus-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}",
        fileSizeLimitBytes: 10485760,
        rollOnFileSizeLimit: true
    )
);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Baðlantý dizesi yapýlandýrýlmadý.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
);

builder.Services.AddDistributedMemoryCache();
Log.Information("In-memory cache kullanýlýyor");

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddHostedService<StockAlertBackgroundService>();

builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Bu alan zorunludur.");
});

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

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
    };
});

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Log.Information("Veritabaný migration'larý baþarýyla uygulandý");
        
        await EnsureDemoAdminUserAsync(context);
        await EnsurePersonalInventoryCategoriesAsync(context);
        await NormalizeLegacyPriceScaleAsync(context, app.Environment.ContentRootPath);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Veritabaný migration uygulanamadý");
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

try
{
    Log.Information("StockifyPlus baþarýyla baþlatýldý");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama baþlatýlamadý");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static async Task EnsureDemoAdminUserAsync(ApplicationDbContext context)
{
    const string demoUsername = "admin";
    const string demoPassword = "Admin123!";
    const string demoEmail = "admin@stockifyplus.local";

    var demoUser = await context.AppUsers.FirstOrDefaultAsync(u => u.Username == demoUsername);

    if (demoUser == null)
    {
        demoUser = new AppUser
        {
            Username = demoUsername,
            PasswordHash = PasswordHasher.HashPassword(demoPassword),
            FullName = "Demo Admin",
            Email = demoEmail,
            Role = UserRole.Admin,
            IsActive = true,
            CreatedDate = DateTime.Now
        };

        await context.AppUsers.AddAsync(demoUser);
        await context.SaveChangesAsync();
        Log.Information("Demo admin kullanýcýsý oluþturuldu (BCrypt hash): {Username}", demoUsername);
        return;
    }

    if (!PasswordHasher.IsBcryptHash(demoUser.PasswordHash))
    {
        demoUser.PasswordHash = PasswordHasher.HashPassword(demoPassword);
        demoUser.Role = UserRole.Admin;
        demoUser.IsActive = true;
        
        if (string.IsNullOrWhiteSpace(demoUser.Email))
        {
            demoUser.Email = demoEmail;
        }

        context.AppUsers.Update(demoUser);
        await context.SaveChangesAsync();
        Log.Information("Demo admin kullanýcýsý BCrypt'e dönüþtürüldü: {Username}", demoUsername);
    }
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
        Log.Information("Legacy fiyat düzeltmesi uygulandý. Güncellenen ürün sayýsý: {ProductCount}", candidates.Count);
    }

    File.WriteAllText(markerPath, $"AppliedAt={DateTime.UtcNow:O}");
}

static async Task EnsurePersonalInventoryCategoriesAsync(ApplicationDbContext context)
{
    var personalCategories = new[]
    {
        new { Name = "3D Printing", Description = "3D yazýcý filamentleri ve malzemeleri (PLA, PETG, TPU)" },
        new { Name = "Supplements", Description = "Spor takviyeleri ve vitaminler (Kreatin, Protein, Vitamin)" },
        new { Name = "Music Equipment", Description = "Müzik ekipmanlarý (Mikrofonlar, Gitarlar, Aksesuarlar)" },
        new { Name = "Motorcycle Parts", Description = "Motosiklet bakým parçalarý ve yaðlar" }
    };

    foreach (var categoryData in personalCategories)
    {
        var existingCategory = await context.Categories
            .FirstOrDefaultAsync(c => c.Name == categoryData.Name);

        if (existingCategory == null)
        {
            var newCategory = new Category
            {
                Name = categoryData.Name,
                Description = categoryData.Description,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            await context.Categories.AddAsync(newCategory);
            Log.Information("Kiþisel envanter kategorisi oluþturuldu: {CategoryName}", categoryData.Name);
        }
    }

    await context.SaveChangesAsync();
}
