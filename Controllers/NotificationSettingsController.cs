using Microsoft.AspNetCore.Mvc;
using StockifyPlus.Models.ViewModels;
using StockifyPlus.Services.Interfaces;

namespace StockifyPlus.Controllers
{
    public class NotificationSettingsController : Controller
    {
        private readonly INotificationSettingService _notificationSettingService;
        private readonly ILogger<NotificationSettingsController> _logger;

        public NotificationSettingsController(
            INotificationSettingService notificationSettingService,
            ILogger<NotificationSettingsController> logger)
        {
            _notificationSettingService = notificationSettingService ?? throw new ArgumentNullException(nameof(notificationSettingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            if (!IsSignedIn())
            {
                TempData["ErrorMessage"] = "Bu alana erismek icin once giris yapiniz.";
                return RedirectToAction("Login", "Account");
            }

            var setting = await _notificationSettingService.GetOrCreateAsync();
            var model = new NotificationSettingsViewModel
            {
                PushEnabled = setting.PushEnabled,
                AlertEmail = setting.AlertEmail
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(NotificationSettingsViewModel model)
        {
            if (!IsSignedIn())
            {
                TempData["ErrorMessage"] = "Bu alana erismek icin once giris yapiniz.";
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var username = HttpContext.Session.GetString("Username") ?? "System";
                await _notificationSettingService.UpdateAsync(model.PushEnabled, model.AlertEmail, username);
                TempData["SuccessMessage"] = "Stok bildirimi ayarlari guncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim ayarlari guncellenirken hata olustu.");
                ModelState.AddModelError(string.Empty, "Ayarlar kaydedilirken hata olustu.");
                return View(model);
            }
        }

        private bool IsSignedIn()
        {
            return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserId"));
        }
    }
}

