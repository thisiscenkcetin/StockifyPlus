using Microsoft.AspNetCore.Mvc;
using StockifyPlus.Services.Interfaces;
using StockifyPlus.Exceptions;

namespace StockifyPlus.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryService.GetAllActiveCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategorileri listelemede hata");
                ModelState.AddModelError("", "Kategorileri listelemede hata oluştu.");
                return View(new List<Models.Category>());
            }
        }
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return View(category);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning($"Kategori bulunamadı: {id}");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kategori detaylarını getirmede hata: {id}");
                ModelState.AddModelError("", "Kategori detaylarını getirmede hata oluştu.");
                return RedirectToAction(nameof(Index));
            }
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    ModelState.AddModelError(nameof(name), "Kategori adı boş bırakılamaz.");
                    return View();
                }

                await _categoryService.CreateCategoryAsync(name, description);
                TempData["SuccessMessage"] = "Kategori başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori oluşturmada hata");
                ModelState.AddModelError("", "Kategori oluşturmada hata oluştu.");
                return View();
            }
        }
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return View(category);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kategori düzenleme sayfasını getirmede hata: {id}");
                ModelState.AddModelError("", "Kategori getirilemedi.");
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string name, string description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    ModelState.AddModelError(nameof(name), "Kategori adı boş bırakılamaz.");
                    var category = await _categoryService.GetCategoryByIdAsync(id);
                    return View(category);
                }

                await _categoryService.UpdateCategoryAsync(id, name, description);
                TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return View(category);
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kategori güncellenmede hata: {id}");
                ModelState.AddModelError("", "Kategori güncellemede hata oluştu.");
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return View(category);
            }
        }
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return View(category);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kategori silme sayfasını getirmede hata: {id}");
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _categoryService.DeactivateCategoryAsync(id);
                TempData["SuccessMessage"] = "Kategori başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BusinessException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return View("Delete", category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kategori silmede hata: {id}");
                ModelState.AddModelError("", "Kategori silmede hata oluştu.");
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return View("Delete", category);
            }
        }
    }
}


