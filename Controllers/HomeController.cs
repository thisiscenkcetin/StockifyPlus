using Microsoft.AspNetCore.Mvc;
using StockifyPlus.Models.Enums;
using StockifyPlus.Services.Interfaces;

namespace StockifyPlus.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IStockMovementService _stockMovementService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, 
            ICategoryService categoryService, IStockMovementService stockMovementService)
        {
            _logger = logger;
            _productService = productService;
            _categoryService = categoryService;
            _stockMovementService = stockMovementService;
        }
        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");
            ViewBag.Username = username;
            
            try
            {
                var activeProducts = await _productService.GetAllActiveProductsAsync();
                var lowStockProducts = await _productService.GetLowStockProductsAsync();
                var activeCategories = await _categoryService.GetAllActiveCategoriesAsync();
                var allMovements = await _stockMovementService.GetAllMovementsAsync();
                
                var todayMovements = allMovements?.Where(m => m.MovementDate.Date == DateTime.Now.Date).Count() ?? 0;
                
                ViewData["TotalProducts"] = activeProducts?.Count() ?? 0;
                ViewData["LowStockProducts"] = lowStockProducts?.Count() ?? 0;
                ViewData["TotalCategories"] = activeCategories?.Count() ?? 0;
                ViewData["TodayMovements"] = todayMovements;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Dashboard veri yükleme hatası: {ex.Message}");
                // Varsayılan değerleri kullan
                ViewData["TotalProducts"] = 0;
                ViewData["LowStockProducts"] = 0;
                ViewData["TotalCategories"] = 0;
                ViewData["TodayMovements"] = 0;
            }
            
            return View();
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
                _logger.LogError(ex, "Canlı aktivite akışı yüklenemedi.");
                return StatusCode(500, new { success = false, message = "Aktivite verileri alınamadı." });
            }
        }

        private static string GetMovementTitle(MovementType movementType)
        {
            return movementType switch
            {
                MovementType.Giriş => "Stok Girişi",
                MovementType.Çıkış => "Stok Çıkışı",
                MovementType.Transfer => "Stok Transfer",
                MovementType.Ayarlama => "Stok Ayarı",
                _ => "Hareket"
            };
        }

        private static string BuildMovementDetail(Models.StockMovement movement)
        {
            var productName = movement.Product?.Name ?? "Ürün";
            var quantityPrefix = movement.MovementType == MovementType.Çıkış ? "-" : "+";
            return $"{productName} ({quantityPrefix}{movement.Quantity})";
        }

        private sealed class ActivityItem
        {
            public string Title { get; set; } = string.Empty;

            public string Detail { get; set; } = string.Empty;

            public DateTime Time { get; set; }
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


