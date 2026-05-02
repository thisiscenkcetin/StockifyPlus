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
                ModelState.AddModelError("", "Kategorileri listelemede hata oluþtu.");
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
            catch (NotFoundException)
            {
                _logger.LogWarning($"Kategori bulunamadý: {id}");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kategori detaylarýný getirmede hata: {id}");
                ModelState.AddModelError("", "Kategori detaylarýný getirmede hata oluþtu.");
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
                    ModelState.AddModelError(nameof(name), "Kategori adý boþ býrakýlamaz.");
                    return View();
                }

                await _categoryService.CreateCategoryAsync(name, description);
                TempData["SuccessMessage"] = "Kategori baþarýyla oluþturuldu.";
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
                _logger.LogError(ex, "Kategori oluþturmada hata");
                ModelState.AddModelError("", "Kategori oluþturmada hata oluþtu.");
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
                _logger.LogError(ex, $"Kategori düzenleme sayfasýný getirmede hata: {id}");
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
                    ModelState.AddModelError(nameof(name), "Kategori adý boþ býrakýlamaz.");
                    var category = await _categoryService.GetCategoryByIdAsync(id);
                    return View(category);
                }

                await _categoryService.UpdateCategoryAsync(id, name, description);
                TempData["SuccessMessage"] = "Kategori baþarýyla güncellendi.";
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
                ModelState.AddModelError("", "Kategori güncellemede hata oluþtu.");
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
                _logger.LogError(ex, $"Kategori silme sayfasýný getirmede hata: {id}");
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
                TempData["SuccessMessage"] = "Kategori baþarýyla silindi.";
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
                ModelState.AddModelError("", "Kategori silmede hata oluþtu.");
                var category = await _categoryService.GetCategoryByIdAsync(id);
                return View("Delete", category);
            }
        }
    }
}

