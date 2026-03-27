using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using StockifyPlus.Data;
using StockifyPlus.Services.Interfaces;
using StockifyPlus.Exceptions;
using StockifyPlus.Models;
using System.Globalization;

namespace StockifyPlus.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            ApplicationDbContext context,
            ILogger<ProductController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllActiveProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürünleri listelemede hata");
                ModelState.AddModelError("", "Ürünleri listelemede hata oluştu.");
                return View(new List<Models.Product>());
            }
        }
        public async Task<IActionResult> LowStock()
        {
            try
            {
                var products = await _productService.GetLowStockProductsAsync();
                ViewBag.Title = "Düşük Stok Ürünleri";
                return View("Index", products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Düşük stok ürünlerini getirmede hata");
                ModelState.AddModelError("", "Düşük stok ürünlerini getirmede hata oluştu.");
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                return View(product);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning($"Ürün bulunamadı: {id}");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ürün detaylarını getirmede hata: {id}");
                ModelState.AddModelError("", "Ürün detaylarını getirmede hata oluştu.");
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> Create()
        {
            try
            {
                var categories = await _categoryService.GetAllActiveCategoriesAsync();
                ViewBag.Categories = categories;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori listesini getirmede hata");
                ModelState.AddModelError("", "Kategorileri getirmede hata oluştu.");
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int categoryId, string name, string sku, string description, 
            string price, int criticalLevel, int stockQuantity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(sku))
                {
                    ModelState.AddModelError("", "Ürün adı ve SKU boş bırakılamaz.");
                    var categories = await _categoryService.GetAllActiveCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View();
                }

                if (!TryParseDecimal(price, out var parsedPrice))
                {
                    ModelState.AddModelError("price", "Fiyat formatı geçersiz. Örnek: 53.400,00");
                    var categories = await _categoryService.GetAllActiveCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View();
                }

                await _productService.CreateProductAsync(categoryId, name, sku, description, parsedPrice, criticalLevel, stockQuantity);
                TempData["SuccessMessage"] = "Ürün başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var categories = await _categoryService.GetAllActiveCategoriesAsync();
                ViewBag.Categories = categories;
                return View();
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var categories = await _categoryService.GetAllActiveCategoriesAsync();
                ViewBag.Categories = categories;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün oluşturmada hata");
                ModelState.AddModelError("", "Ürün oluşturmada hata oluştu.");
                var categories = await _categoryService.GetAllActiveCategoriesAsync();
                ViewBag.Categories = categories;
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCreateCategory(string name, string description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { success = false, message = "Kategori adı boş olamaz." });
                }

                var category = await _categoryService.CreateCategoryAsync(name, description);
                return Json(new { success = true, id = category.Id, name = category.Name });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hızlı kategori oluşturmada hata");
                return StatusCode(500, new { success = false, message = "Kategori oluşturulamadı." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetProductByBarcode([FromBody] BarcodeLookupRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Barcode))
                {
                    return BadRequest(new { success = false, message = "Barkod boş olamaz." });
                }

                var barcode = request.Barcode.Trim();
                var products = await _productService.GetAllActiveProductsAsync();

                var product = products.FirstOrDefault(p =>
                    !string.IsNullOrWhiteSpace(p.SKU) &&
                    string.Equals(p.SKU.Trim(), barcode, StringComparison.OrdinalIgnoreCase));

                if (product == null)
                {
                    return NotFound(new { success = false, message = "Bu barkoda ait aktif ürün bulunamadı." });
                }

                return Json(new
                {
                    success = true,
                    id = product.Id,
                    name = product.Name,
                    currentStock = product.StockQuantity,
                    sku = product.SKU
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Barkod ile ürün arama sırasında hata oluştu.");
                return StatusCode(500, new { success = false, message = "Barkod arama işlemi sırasında sunucu hatası oluştu." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Urunler");

                worksheet.Cells[1, 1].Value = "Stok Kodu (SKU)";
                worksheet.Cells[1, 2].Value = "Ürün Adı";
                worksheet.Cells[1, 3].Value = "Kategori";
                worksheet.Cells[1, 4].Value = "Fiyat";
                worksheet.Cells[1, 5].Value = "Miktar";

                using (var headerRange = worksheet.Cells[1, 1, 1, 5])
                {
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                var row = 2;
                foreach (var product in products)
                {
                    worksheet.Cells[row, 1].Value = product.SKU;
                    worksheet.Cells[row, 2].Value = product.Name;
                    worksheet.Cells[row, 3].Value = product.Category?.Name ?? string.Empty;
                    worksheet.Cells[row, 4].Value = product.Price;
                    worksheet.Cells[row, 5].Value = product.StockQuantity;
                    row++;
                }

                worksheet.Column(4).Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var bytes = package.GetAsByteArray();
                var fileName = $"StockifyPlus_Urunler_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                return File(bytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel dışa aktarım sırasında hata oluştu.");
                TempData["ErrorMessage"] = "Excel dışa aktarım işlemi başarısız oldu.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Lütfen geçerli bir Excel dosyası seçin." });
                }

                if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { success = false, message = "Sadece .xlsx uzantılı dosyalar desteklenir." });
                }

                var categories = await _context.Categories.ToListAsync();
                var categoryMap = categories
                    .GroupBy(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var products = await _context.Products.ToListAsync();
                var productMap = products
                    .Where(p => !string.IsNullOrWhiteSpace(p.SKU))
                    .GroupBy(p => p.SKU.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                var createdCount = 0;
                var updatedCount = 0;
                var skippedCount = 0;
                var errors = new List<string>();

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null || worksheet.Dimension == null || worksheet.Dimension.Rows < 2)
                {
                    return BadRequest(new { success = false, message = "Excel dosyasında işlenecek satır bulunamadı." });
                }

                var rowCount = worksheet.Dimension.Rows;

                for (var row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var sku = worksheet.Cells[row, 1].Text?.Trim();
                        var name = worksheet.Cells[row, 2].Text?.Trim();
                        var categoryName = worksheet.Cells[row, 3].Text?.Trim();
                        var priceText = worksheet.Cells[row, 4].Text?.Trim();
                        var quantityText = worksheet.Cells[row, 5].Text?.Trim();

                        if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(categoryName))
                        {
                            skippedCount++;
                            errors.Add($"Satır {row}: SKU, Ürün Adı ve Kategori zorunludur.");
                            continue;
                        }

                        if (!TryParseDecimal(priceText, out var price))
                        {
                            skippedCount++;
                            errors.Add($"Satır {row}: Fiyat değeri geçersiz ({priceText}).");
                            continue;
                        }

                        if (!int.TryParse(quantityText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantity))
                        {
                            skippedCount++;
                            errors.Add($"Satır {row}: Miktar değeri geçersiz ({quantityText}).");
                            continue;
                        }

                        if (!categoryMap.TryGetValue(categoryName, out var category))
                        {
                            category = new Category
                            {
                                Name = categoryName,
                                Description = "Excel import ile oluşturuldu.",
                                IsActive = true
                            };

                            await _context.Categories.AddAsync(category);
                            categoryMap[categoryName] = category;
                        }
                        else if (!category.IsActive)
                        {
                            category.IsActive = true;
                        }

                        var normalizedSku = sku.ToUpperInvariant();

                        if (productMap.TryGetValue(normalizedSku, out var existingProduct))
                        {
                            existingProduct.Name = name;
                            existingProduct.Price = price;
                            existingProduct.StockQuantity = Math.Max(0, existingProduct.StockQuantity + quantity);
                            existingProduct.Category = category;
                            existingProduct.Description = string.IsNullOrWhiteSpace(existingProduct.Description)
                                ? "Excel import güncellemesi"
                                : existingProduct.Description;
                            existingProduct.IsActive = true;
                            updatedCount++;
                        }
                        else
                        {
                            var newProduct = new Product
                            {
                                Name = name,
                                SKU = normalizedSku,
                                Price = price,
                                StockQuantity = Math.Max(0, quantity),
                                CriticalStockLevel = 10,
                                Category = category,
                                Description = "Excel import ile eklendi.",
                                IsActive = true
                            };

                            await _context.Products.AddAsync(newProduct);
                            productMap[normalizedSku] = newProduct;
                            createdCount++;
                        }
                    }
                    catch (Exception rowEx)
                    {
                        skippedCount++;
                        errors.Add($"Satır {row}: {rowEx.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Excel içeri aktarma tamamlandı.",
                    totalRows = rowCount - 1,
                    created = createdCount,
                    updated = updatedCount,
                    skipped = skippedCount,
                    errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel içeri aktarma sırasında hata oluştu.");
                return StatusCode(500, new { success = false, message = "Excel içeri aktarma sırasında sunucu hatası oluştu." });
            }
        }
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                var categories = await _categoryService.GetAllActiveCategoriesAsync();
                ViewBag.Categories = categories;
                return View(product);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ürün düzenleme sayfasını getirmede hata: {id}");
                ModelState.AddModelError("", "Ürün getirilemedi.");
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, int categoryId, string name, string sku, string description, 
            string price, int criticalLevel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(sku))
                {
                    ModelState.AddModelError("", "Ürün adı ve SKU boş bırakılamaz.");
                    var product = await _productService.GetProductByIdAsync(id);
                    var categories = await _categoryService.GetAllActiveCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View(product);
                }

                if (!TryParseDecimal(price, out var parsedPrice))
                {
                    ModelState.AddModelError("price", "Fiyat formatı geçersiz. Örnek: 53.400,00");
                    var product = await _productService.GetProductByIdAsync(id);
                    var categories = await _categoryService.GetAllActiveCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View(product);
                }

                await _productService.UpdateProductAsync(id, categoryId, name, sku, description, parsedPrice, criticalLevel);
                TempData["SuccessMessage"] = "Ürün başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var product = await _productService.GetProductByIdAsync(id);
                var categories = await _categoryService.GetAllActiveCategoriesAsync();
                ViewBag.Categories = categories;
                return View(product);
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var product = await _productService.GetProductByIdAsync(id);
                var categories = await _categoryService.GetAllActiveCategoriesAsync();
                ViewBag.Categories = categories;
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ürün güncellemede hata: {id}");
                ModelState.AddModelError("", "Ürün güncellemede hata oluştu.");
                var product = await _productService.GetProductByIdAsync(id);
                var categories = await _categoryService.GetAllActiveCategoriesAsync();
                ViewBag.Categories = categories;
                return View(product);
            }
        }
        public async Task<IActionResult> Barcode(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                return View(product);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ürün barkod sayfasını getirmede hata: {id}");
                TempData["ErrorMessage"] = "Barkod sayfası açılamadı.";
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                return View(product);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ürün silme sayfasını getirmede hata: {id}");
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _productService.DeactivateProductAsync(id);
                TempData["SuccessMessage"] = "Ürün başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ürün silmede hata: {id}");
                ModelState.AddModelError("", "Ürün silmede hata oluştu.");
                var product = await _productService.GetProductByIdAsync(id);
                return View(product);
            }
        }

        public sealed class BarcodeLookupRequest
        {
            public string Barcode { get; set; } = string.Empty;
        }

        private static bool TryParseDecimal(string? value, out decimal result)
        {
            result = 0;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var sanitized = value
                .Trim()
                .Replace("₺", string.Empty)
                .Replace("TL", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace(" ", string.Empty);

            var normalized = sanitized;
            var hasDot = sanitized.Contains('.');
            var hasComma = sanitized.Contains(',');

            if (hasDot && hasComma)
            {
                if (sanitized.LastIndexOf(',') > sanitized.LastIndexOf('.'))
                {
                    normalized = sanitized.Replace(".", string.Empty).Replace(',', '.');
                }
                else
                {
                    normalized = sanitized.Replace(",", string.Empty);
                }
            }
            else if (hasComma)
            {
                normalized = sanitized.Replace(',', '.');
            }
            else if (hasDot && sanitized.Count(c => c == '.') > 1)
            {
                normalized = sanitized.Replace(".", string.Empty);
            }

            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out result)
                || decimal.TryParse(sanitized, NumberStyles.Number, CultureInfo.InvariantCulture, out result)
                || decimal.TryParse(sanitized, NumberStyles.Number, CultureInfo.GetCultureInfo("tr-TR"), out result);
        }
    }
}

