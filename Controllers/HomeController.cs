using Microsoft.AspNetCore.Mvc;
using StockifyPlus.Models.Enums;
using StockifyPlus.Services.Interfaces;
using System.Text.RegularExpressions;

namespace StockifyPlus.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IStockMovementService _stockMovementService;
        private readonly ICacheService _cacheService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, 
            ICategoryService categoryService, IStockMovementService stockMovementService,
            ICacheService cacheService)
        {
            _logger = logger;
            _productService = productService;
            _categoryService = categoryService;
            _stockMovementService = stockMovementService;
            _cacheService = cacheService;
        }
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");
            ViewBag.Username = username;
            
            try
            {
                const string cacheKeyDashboard = "dashboard:kpi-metrics";
                
                var cachedMetrics = await _cacheService.GetAsync<DashboardMetrics>(cacheKeyDashboard);
                
                if (cachedMetrics != null)
                {
                    _logger.LogInformation("Dashboard metrikleri cache'den yüklendi.");
                    ViewData["TotalProducts"] = cachedMetrics.TotalProducts;
                    ViewData["LowStockProducts"] = cachedMetrics.LowStockProducts;
                    ViewData["TotalCategories"] = cachedMetrics.TotalCategories;
                    ViewData["TodayMovements"] = cachedMetrics.TodayMovements;
                }
                else
                {
                    _logger.LogInformation("Dashboard metrikleri veritabanýndan yükleniyor...");
                    
                    var activeProducts = await _productService.GetAllActiveProductsAsync();
                    var lowStockProducts = await _productService.GetLowStockProductsAsync();
                    var activeCategories = await _categoryService.GetAllActiveCategoriesAsync();
                    var allMovements = await _stockMovementService.GetAllMovementsAsync();
                    
                    var todayMovements = allMovements?.Where(m => m.MovementDate.Date == DateTime.Now.Date).Count() ?? 0;
                    
                    var metrics = new DashboardMetrics
                    {
                        TotalProducts = activeProducts?.Count() ?? 0,
                        LowStockProducts = lowStockProducts?.Count() ?? 0,
                        TotalCategories = activeCategories?.Count() ?? 0,
                        TodayMovements = todayMovements
                    };
                    
                    await _cacheService.SetAsync(cacheKeyDashboard, metrics, TimeSpan.FromMinutes(5));
                    
                    ViewData["TotalProducts"] = metrics.TotalProducts;
                    ViewData["LowStockProducts"] = metrics.LowStockProducts;
                    ViewData["TotalCategories"] = metrics.TotalCategories;
                    ViewData["TodayMovements"] = metrics.TodayMovements;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dashboard veri yükleme hatasý: {ex.Message}");
                ViewData["TotalProducts"] = 0;
                ViewData["LowStockProducts"] = 0;
                ViewData["TotalCategories"] = 0;
                ViewData["TodayMovements"] = 0;
            }
            
            return View();
        }
        
        private class DashboardMetrics
        {
            public int TotalProducts { get; set; }
            public int LowStockProducts { get; set; }
            public int TotalCategories { get; set; }
            public int TodayMovements { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> RecentActivities(int take = 8)
        {
            try
            {
                if (take <= 0)
                {
                    take = 8;
                }

                take = Math.Min(take, 30);

                var movements = await _stockMovementService.GetAllMovementsAsync();
                var products = await _productService.GetAllActiveProductsAsync();
                var categories = await _categoryService.GetAllActiveCategoriesAsync();

                var movementActivities = movements.Select(m => new ActivityItem
                {
                    Title = GetMovementTitle(m.MovementType),
                    Detail = BuildMovementDetail(m),
                    Time = m.MovementDate
                });

                var productActivities = products.Select(p => new ActivityItem
                {
                    Title = "Ürün Eklendi",
                    Detail = p.Name,
                    Time = p.CreatedDate
                });

                var categoryActivities = categories.Select(c => new ActivityItem
                {
                    Title = "Kategori Eklendi",
                    Detail = c.Name,
                    Time = c.CreatedDate
                });

                var activities = movementActivities
                    .Concat(productActivities)
                    .Concat(categoryActivities)
                    .OrderByDescending(a => a.Time)
                    .Take(take)
                    .Select(a => new
                    {
                        title = a.Title,
                        detail = a.Detail,
                        time = a.Time
                    })
                    .ToList();

                return Json(new { success = true, items = activities });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Canlý aktivite akýþý yüklenemedi.");
                return StatusCode(500, new { success = false, message = "Aktivite verileri alýnamadý." });
            }
        }

        private static string GetMovementTitle(MovementType movementType)
        {
            return movementType switch
            {
                MovementType.Giriþ => "Stok Giriþi",
                MovementType.Çýkýþ => "Stok Çýkýþý",
                MovementType.Transfer => "Stok Transfer",
                MovementType.Ayarlama => "Stok Ayarý",
                _ => "Hareket"
            };
        }

        private static string BuildMovementDetail(Models.StockMovement movement)
        {
            var productName = movement.Product?.Name ?? "Ürün";
            var quantityPrefix = movement.MovementType == MovementType.Çýkýþ ? "-" : "+";
            return $"{productName} ({quantityPrefix}{movement.Quantity})";
        }

        private sealed class ActivityItem
        {
            public string Title { get; set; } = string.Empty;

            public string Detail { get; set; } = string.Empty;

            public DateTime Time { get; set; }
        }
        [HttpGet]
        public IActionResult Logs(string? level = null, string? source = null, string? q = null, int take = 500)
        {
            take = Math.Clamp(take, 100, 2000);

            var files = GetLogFiles();
            var entries = files
                .SelectMany(file => ReadLogEntries(file))
                .OrderByDescending(entry => entry.Timestamp ?? DateTimeOffset.MinValue)
                .ThenByDescending(entry => entry.FileName)
                .ToList();

            var filtered = entries.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(level))
            {
                filtered = filtered.Where(entry => string.Equals(entry.Level, level, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(source))
            {
                filtered = filtered.Where(entry => entry.Source.Contains(source, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                filtered = filtered.Where(entry =>
                    entry.Message.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    entry.Source.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    entry.FileName.Contains(q, StringComparison.OrdinalIgnoreCase));
            }

            var filteredList = filtered.Take(take).ToList();

            var model = new LogsPageViewModel
            {
                Level = level ?? string.Empty,
                Source = source ?? string.Empty,
                Query = q ?? string.Empty,
                Take = take,
                TotalCount = entries.Count,
                FilteredCount = filtered.Count(),
                Files = files.Select(Path.GetFileName).Where(name => !string.IsNullOrWhiteSpace(name)).Cast<string>().ToList(),
                Levels = entries.Select(entry => entry.Level)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .ToList(),
                Sources = entries.Select(entry => entry.Source)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .Take(30)
                    .ToList(),
                Entries = filteredList
            };

            return View(model);
        }

        private List<string> GetLogFiles()
        {
            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(logDirectory))
            {
                return new List<string>();
            }

            return Directory.GetFiles(logDirectory, "*.log")
                .OrderByDescending(System.IO.File.GetLastWriteTimeUtc)
                .Take(40)
                .ToList();
        }

        private static IEnumerable<LogEntryViewModel> ReadLogEntries(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            return ReadAllLinesShared(filePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => ParseLogLine(line, fileName));
        }

        private static string[] ReadAllLinesShared(string filePath)
        {
            try
            {
                using var stream = new FileStream(filePath, System.IO.FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();
                return content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        private static LogEntryViewModel ParseLogLine(string line, string fileName)
        {
            var fileLogMatch = Regex.Match(line, @"^(?<timestamp>\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\.\d{3}\s+[+-]\d{2}:\d{2})\s+\[(?<level>[A-Z]{3})\]\s+(?<source>.*?)\s+-\s+(?<message>.*)$");
            if (fileLogMatch.Success)
            {
                DateTimeOffset.TryParse(fileLogMatch.Groups["timestamp"].Value, out var timestamp);
                var source = fileLogMatch.Groups["source"].Value.Trim();
                var message = fileLogMatch.Groups["message"].Value.Trim();
                return new LogEntryViewModel
                {
                    Timestamp = timestamp,
                    Level = fileLogMatch.Groups["level"].Value,
                    Source = source,
                    Message = message,
                    FileName = fileName,
                    Category = ResolveLogCategory(source, message)
                };
            }

            var consoleMatch = Regex.Match(line, @"^\[(?<time>\d{2}:\d{2}:\d{2})\s+(?<level>[A-Z]{3})\]\s*(?<source>.*)$");
            if (consoleMatch.Success)
            {
                var today = DateTimeOffset.Now.Date;
                DateTimeOffset.TryParse($"{today:yyyy-MM-dd} {consoleMatch.Groups["time"].Value} {DateTimeOffset.Now:zzz}", out var timestamp);
                var source = consoleMatch.Groups["source"].Value.Trim();
                return new LogEntryViewModel
                {
                    Timestamp = timestamp,
                    Level = consoleMatch.Groups["level"].Value,
                    Source = string.IsNullOrWhiteSpace(source) ? "Application" : source,
                    Message = line,
                    FileName = fileName,
                    Category = ResolveLogCategory(source, line)
                };
            }

            return new LogEntryViewModel
            {
                Level = "LOG",
                Source = "Raw",
                Message = line,
                FileName = fileName,
                Category = ResolveLogCategory("Raw", line)
            };
        }

        private static string ResolveLogCategory(string source, string message)
        {
            var text = $"{source} {message}";

            if (text.Contains("HTTP ", StringComparison.OrdinalIgnoreCase))
            {
                return "HTTP";
            }

            if (text.Contains("Login", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Logout", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Account", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Giriþ", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Giris", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Kullanýcý", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Kullanici", StringComparison.OrdinalIgnoreCase))
            {
                return "Giris";
            }

            if (text.Contains("StockAlert", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Kritik stok", StringComparison.OrdinalIgnoreCase))
            {
                return "Bildirim";
            }

            if (text.Contains("Dashboard", StringComparison.OrdinalIgnoreCase))
            {
                return "Dashboard";
            }

            if (text.Contains("migration", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Veritab", StringComparison.OrdinalIgnoreCase))
            {
                return "Veritabani";
            }

            return "Sistem";
        }

        public sealed class LogsPageViewModel
        {
            public string Level { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
            public string Query { get; set; } = string.Empty;
            public int Take { get; set; }
            public int TotalCount { get; set; }
            public int FilteredCount { get; set; }
            public List<string> Files { get; set; } = new();
            public List<string> Levels { get; set; } = new();
            public List<string> Sources { get; set; } = new();
            public List<LogEntryViewModel> Entries { get; set; } = new();
        }

        public sealed class LogEntryViewModel
        {
            public DateTimeOffset? Timestamp { get; set; }
            public string Level { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string FileName { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Error()
        {
            return View();
        }
    }
}

