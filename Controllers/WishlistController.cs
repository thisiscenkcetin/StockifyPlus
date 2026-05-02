using Microsoft.AspNetCore.Mvc;
using StockifyPlus.Models;
using StockifyPlus.Services.Interfaces;

namespace StockifyPlus.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishlistService;
        private readonly ILogger<WishlistController> _logger;

        public WishlistController(IWishlistService wishlistService, ILogger<WishlistController> logger)
        {
            _wishlistService = wishlistService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);
            var wishlists = await _wishlistService.GetActiveWishlistAsync(userId);

            ViewBag.Title = "İstek Listem";
            return View(wishlists);
        }

        public IActionResult Create()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Title = "Yeni İstek Ekle";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Wishlist wishlist)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    wishlist.UserId = int.Parse(userIdString);
                    await _wishlistService.CreateAsync(wishlist);

                    TempData["SuccessMessage"] = "İstek başarıyla eklendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Wishlist oluşturulurken hata oluştu");
                    ModelState.AddModelError("", "İstek eklenirken bir hata oluştu.");
                }
            }

            ViewBag.Title = "Yeni İstek Ekle";
            return View(wishlist);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlist = await _wishlistService.GetByIdAsync(id);
            if (wishlist == null)
            {
                return NotFound();
            }

            var userId = int.Parse(userIdString);
            if (wishlist.UserId != userId)
            {
                return Forbid();
            }

            ViewBag.Title = "İstek Düzenle";
            return View(wishlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Wishlist wishlist)
        {
            if (id != wishlist.Id)
            {
                return NotFound();
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);
            if (wishlist.UserId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _wishlistService.UpdateAsync(wishlist);
                    TempData["SuccessMessage"] = "İstek başarıyla güncellendi!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Wishlist güncellenirken hata oluştu");
                    ModelState.AddModelError("", "İstek güncellenirken bir hata oluştu.");
                }
            }

            ViewBag.Title = "İstek Düzenle";
            return View(wishlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var wishlist = await _wishlistService.GetByIdAsync(id);
                if (wishlist == null)
                {
                    return NotFound();
                }

                var userId = int.Parse(userIdString);
                if (wishlist.UserId != userId)
                {
                    return Forbid();
                }

                await _wishlistService.DeleteAsync(id);
                TempData["SuccessMessage"] = "İstek başarıyla silindi!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wishlist silinirken hata oluştu");
                TempData["ErrorMessage"] = "İstek silinirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPurchased(int id)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrWhiteSpace(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var wishlist = await _wishlistService.GetByIdAsync(id);
                if (wishlist == null)
                {
                    return NotFound();
                }

                var userId = int.Parse(userIdString);
                if (wishlist.UserId != userId)
                {
                    return Forbid();
                }

                await _wishlistService.MarkAsPurchasedAsync(id);
                TempData["SuccessMessage"] = "İstek satın alındı olarak işaretlendi!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wishlist satın alındı olarak işaretlenirken hata oluştu");
                TempData["ErrorMessage"] = "İşlem sırasında bir hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
