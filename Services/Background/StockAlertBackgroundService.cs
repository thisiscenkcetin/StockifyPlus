using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StockifyPlus.Configurations;
using StockifyPlus.Data;
using StockifyPlus.Models;
using StockifyPlus.Services.Interfaces;
using System.Text;

namespace StockifyPlus.Services.Background
{
    public class StockAlertBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly SmtpOptions _smtpOptions;
        private readonly ILogger<StockAlertBackgroundService> _logger;

        public StockAlertBackgroundService(
            IServiceScopeFactory scopeFactory,
            IOptions<SmtpOptions> smtpOptions,
            ILogger<StockAlertBackgroundService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _smtpOptions = smtpOptions?.Value ?? throw new ArgumentNullException(nameof(smtpOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StockAlertBackgroundService baslatildi.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextRun = now.Date.AddDays(1); // bir sonraki gece 00:00
                    var delay = nextRun - now;

                    _logger.LogInformation("Kritik stok taramasi icin sonraki calisma: {NextRun}", nextRun);
                    await Task.Delay(delay, stoppingToken);

                    await ProcessCriticalStockReportAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kritik stok worker dongusunde beklenmeyen hata olustu.");
                }
            }

            _logger.LogInformation("StockAlertBackgroundService durduruldu.");
        }

        private async Task ProcessCriticalStockReportAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var notificationSettingService = scope.ServiceProvider.GetRequiredService<INotificationSettingService>();

            var alertEmail = await notificationSettingService.GetAlertEmailAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(alertEmail))
            {
                alertEmail = _smtpOptions.AdminEmail;
            }

            if (string.IsNullOrWhiteSpace(alertEmail))
            {
                _logger.LogWarning("Kritik stok raporu icin alici e-posta adresi bulunamadi.");
                return;
            }

            var criticalProducts = await dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.StockQuantity <= p.CriticalStockLevel)
                .OrderBy(p => p.StockQuantity)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);

            if (criticalProducts.Count == 0)
            {
                _logger.LogInformation("Kritik stokta urun bulunmadi. E-posta gonderimi atlandi.");
                return;
            }

            var subject = "DIKKAT: StockifyPlus Gunluk Kritik Stok Raporu";
            var htmlBody = BuildCriticalStockHtml(criticalProducts);

            await emailService.SendHtmlEmailAsync(alertEmail, subject, htmlBody, cancellationToken);
            _logger.LogInformation("Kritik stok raporu gonderildi. Urun sayisi: {Count}", criticalProducts.Count);
        }

        private static string BuildCriticalStockHtml(IEnumerable<Product> products)
        {
            var rows = new StringBuilder();

            foreach (var product in products)
            {
                rows.Append($"<tr><td>{product.Name}</td><td>{product.SKU}</td><td>{product.Category?.Name ?? "-"}</td><td>{product.StockQuantity}</td><td>{product.CriticalStockLevel}</td></tr>");
            }

            return $@"
                <div style='font-family:Segoe UI,Arial,sans-serif;color:#1f2937;'>
                    <h2 style='margin-bottom:8px;'>DIKKAT: StockifyPlus Gunluk Kritik Stok Raporu</h2>
                    <p style='margin-top:0;margin-bottom:16px;'>Kritik seviyede bulunan urunler asagidadir:</p>
                    <table style='border-collapse:collapse;width:100%;'>
                        <thead>
                            <tr style='background:#f3f4f6;'>
                                <th style='text-align:left;padding:8px;border:1px solid #e5e7eb;'>Urun</th>
                                <th style='text-align:left;padding:8px;border:1px solid #e5e7eb;'>SKU</th>
                                <th style='text-align:left;padding:8px;border:1px solid #e5e7eb;'>Kategori</th>
                                <th style='text-align:right;padding:8px;border:1px solid #e5e7eb;'>Mevcut Stok</th>
                                <th style='text-align:right;padding:8px;border:1px solid #e5e7eb;'>Kritik Seviye</th>
                            </tr>
                        </thead>
                        <tbody>
                            {rows}
                        </tbody>
                    </table>
                    <p style='margin-top:16px;color:#6b7280;'>Bu e-posta StockifyPlus tarafindan otomatik olusturulmustur.</p>
                </div>";
        }
    }
}
