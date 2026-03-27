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
                    ModelState.AddModelError(nameof(username), "Kullanıcı adı boş bırakılamaz.");
                    return View();
                }

                if (password != confirmPassword)
                {
                    ModelState.AddModelError("", "�?ifreler eşleşmiyor.");
                    return View();
                }

                var user = await _accountService.RegisterAsync(username, password, fullName, email, UserRole.DepoPersoneli);
                TempData["SuccessMessage"] = "Hesap başarıyla oluşturuldu. Giriş yapabilirsiniz.";
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
                _logger.LogError(ex, "Kullanıcı kayıtlarken hata");
                ModelState.AddModelError("", "Kayıt sırasında hata oluştu.");
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
                    ModelState.AddModelError("", "Kullanıcı adı ve şifre boş bırakılamaz.");
                    return View();
                }

                var user = await _accountService.LoginAsync(username, password);
                
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserRole", user.Role.ToString());
                
                TempData["SuccessMessage"] = $"Hoşgeldiniz {user.Username}!";
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
            catch (NotFoundException ex)
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giriş sırasında hata");
                ModelState.AddModelError("", "Giriş sırasında hata oluştu.");
                return View();
            }
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Çıkış yapıldı.";
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
                    ModelState.AddModelError("", "Yeni şifreler eşleşmiyor.");
                    return View();
                }

                await _accountService.ChangePasswordAsync(userId, oldPassword, newPassword);
                TempData["SuccessMessage"] = "�?ifre başarıyla değiştirildi.";
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
                _logger.LogError(ex, "�?ifre değiştirilirken hata");
                ModelState.AddModelError("", "�?ifre değiştirilirken hata oluştu.");
                return View();
            }
        }
    }
}


