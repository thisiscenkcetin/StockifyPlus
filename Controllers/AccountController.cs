using Microsoft.AspNetCore.Mvc;
using StockifyPlus.Services.Interfaces;
using StockifyPlus.Exceptions;
using StockifyPlus.Models.Enums;

namespace StockifyPlus.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string password, string confirmPassword, 
            string fullName, string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    ModelState.AddModelError(nameof(username), "Kullanýcý adý boþ býrakýlamaz.");
                    return View();
                }

                if (password != confirmPassword)
                {
                    ModelState.AddModelError("", "??ifreler eþleþmiyor.");
                    return View();
                }

                var user = await _accountService.RegisterAsync(username, password, fullName, email, UserRole.DepoPersoneli);
                TempData["SuccessMessage"] = "Hesap baþarýyla oluþturuldu. Giriþ yapabilirsiniz.";
                return RedirectToAction(nameof(Login));
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
                _logger.LogError(ex, "Kullanýcý kayýtlarken hata");
                ModelState.AddModelError("", "Kayýt sýrasýnda hata oluþtu.");
                return View();
            }
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    ModelState.AddModelError("", "Kullanýcý adý ve þifre boþ býrakýlamaz.");
                    return View();
                }

                var user = await _accountService.LoginAsync(username, password);
                
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserRole", user.Role.ToString());
                
                return RedirectToAction("Index", "Home");
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
            catch (NotFoundException)
            {
                ModelState.AddModelError("", "Kullanýcý adý veya þifre hatalý.");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giriþ sýrasýnda hata");
                ModelState.AddModelError("", "Giriþ sýrasýnda hata oluþtu.");
                return View();
            }
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Çýkýþ yapýldý.";
            return RedirectToAction(nameof(Login));
        }
        public async Task<IActionResult> Profile()
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return RedirectToAction(nameof(Login));
                }

                var user = await _accountService.GetUserByIdAsync(userId);
                return View(user);
            }
            catch (NotFoundException)
            {
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil getirilemedi");
                ModelState.AddModelError("", "Profil getirilemedi.");
                return RedirectToAction("Index", "Home");
            }
        }
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                {
                    return RedirectToAction(nameof(Login));
                }

                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("", "Yeni þifreler eþleþmiyor.");
                    return View();
                }

                await _accountService.ChangePasswordAsync(userId, oldPassword, newPassword);
                TempData["SuccessMessage"] = "??ifre baþarýyla deðiþtirildi.";
                return RedirectToAction(nameof(Profile));
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
                _logger.LogError(ex, "??ifre deðiþtirilirken hata");
                ModelState.AddModelError("", "??ifre deðiþtirilirken hata oluþtu.");
                return View();
            }
        }
    }
}

