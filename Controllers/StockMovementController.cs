using Microsoft.AspNetCore.Mvc;
using StockifyPlus.Services.Interfaces;
using StockifyPlus.Exceptions;

namespace StockifyPlus.Controllers
{
    public class StockMovementController : Controller
    {
        private readonly IStockMovementService _stockMovementService;
        private readonly IProductService _productService;
        private readonly ILogger<StockMovementController> _logger;

        public StockMovementController(
            IStockMovementService stockMovementService,
            IProductService productService,
            ILogger<StockMovementController> logger)
        {
            _stockMovementService = stockMovementService ?? throw new ArgumentNullException(nameof(stockMovementService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var movements = await _stockMovementService.GetAllMovementsAsync();
                return View(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok hareketlerini listelemede hata");
                ModelState.AddModelError("", "Stok hareketlerini listelemede hata oluştu.");
                return View(new List<Models.StockMovement>());
            }
        }
        public async Task<IActionResult> ProductHistory(int productId)
        {
            try
            {
                var movements = await _stockMovementService.GetMovementsByProductAsync(productId);
                var product = await _productService.GetProductByIdAsync(productId);
                ViewBag.Product = product;
                return View(movements);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ürün hareketlerini getirmede hata: {productId}");
                ModelState.AddModelError("", "Ürün hareketlerini getirmede hata oluştu.");
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> DateRange(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                if (!startDate.HasValue)
                    startDate = DateTime.Now.AddDays(-30);
                if (!endDate.HasValue)
                    endDate = DateTime.Now;

                var movements = await _stockMovementService.GetMovementsByDateRangeAsync(startDate.Value, endDate.Value);
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                return View(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tarih aralığı hareketlerini getirmede hata");
                ModelState.AddModelError("", "Hareketleri getirmede hata oluştu.");
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> StockIn()
        {
            try
            {
                var products = await _productService.GetAllActiveProductsAsync();
                ViewBag.Products = products;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün listesini getirmede hata");
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StockIn(int productId, int quantity, string description)
        {
            try
            {
                if (quantity <= 0)
                {
                    ModelState.AddModelError(nameof(quantity), "Giriş miktarı 0'dan büyük olmalıdır.");
                    var products = await _productService.GetAllActiveProductsAsync();
                    ViewBag.Products = products;
                    return View();
                }

                await _stockMovementService.RecordStockInAsync(productId, quantity, description);
                TempData["SuccessMessage"] = $"{quantity} ürün başarıyla stoka girildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var products = await _productService.GetAllActiveProductsAsync();
                ViewBag.Products = products;
                return View();
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var products = await _productService.GetAllActiveProductsAsync();
                ViewBag.Products = products;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok girişi kaydederken hata");
                ModelState.AddModelError("", "Stok girişi kaydederken hata oluştu.");
                var products = await _productService.GetAllActiveProductsAsync();
                ViewBag.Products = products;
                return View();
            }
        }
        public async Task<IActionResult> StockOut()
        {
            try
            {
                var products = await _productService.GetAllActiveProductsAsync();
                ViewBag.Products = products;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün listesini getirmede hata");
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StockOut(int productId, int quantity, string description)
        {
            try
            {
                if (quantity <= 0)
                {
                    ModelState.AddModelError(nameof(quantity), "Çıkış miktarı 0'dan büyük olmalıdır.");
                    var products = await _productService.GetAllActiveProductsAsync();
                    ViewBag.Products = products;
                    return View();
                }

                await _stockMovementService.RecordStockOutAsync(productId, quantity, description);
                TempData["SuccessMessage"] = $"{quantity} ürün başarıyla stoktan çıkartıldı.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var products = await _productService.GetAllActiveProductsAsync();
                ViewBag.Products = products;
                return View();
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var products = await _productService.GetAllActiveProductsAsync();
                ViewBag.Products = products;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok çıkışı kaydederken hata");
                ModelState.AddModelError("", "Stok çıkışı kaydederken hata oluştu.");
                var products = await _productService.GetAllActiveProductsAsync();
                ViewBag.Products = products;
                return View();
            }
        }
        public async Task<IActionResult> Adjustment()
        {
            try
            {
                var products = await _productService.GetAllActiveProductsAsync();
                ViewBag.Products = products;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün listesini getirmede hata");
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjustment(int productId, int quantity, string description)
        {
            try
            {
                // Miktar burada sifir/negatif gelebiliyor, servis tarafi buna gore hareket tipi secsin diye boyle biraktim.
                await _stockMovementService.RecordStockAdjustmentAsync(productId, quantity, description);
                TempData["SuccessMessage"] = "Stok ayarlaması başarıyla kaydedildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var products = await _productService.GetAllActiveProductsAsync();
                ViewBag.Products = products;
                return View();
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var products = await _productService.GetAllActiveProductsAsync();
                ViewBag.Products = products;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stok ayarlaması kaydederken hata");
                ModelState.AddModelError("", "Stok ayarlaması kaydederken hata oluştu.");
                var products = await _productService.GetAllActiveProductsAsync();
                ViewBag.Products = products;
                return View();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReverseMovement(int movementId)
        {
            try
            {
                await _stockMovementService.ReverseMovementAsync(movementId);
                TempData["SuccessMessage"] = "Stok hareketi başarıyla geri alındı.";
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                TempData["ErrorMessage"] = "Hareket bulunamadı.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Hareketi geri almada hata: {movementId}");
                TempData["ErrorMessage"] = "Hareket geri alınırken hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}


