using Microsoft.AspNetCore.Mvc;
using System.Net;
using StockifyPlus.Services.Interfaces;

namespace StockifyPlus.Controllers
{
    public class StockAIController : Controller
    {
        private readonly IStockAiAgentService _stockAiAgentService;
        private readonly ILogger<StockAIController> _logger;

        public StockAIController(
            IStockAiAgentService stockAiAgentService,
            ILogger<StockAIController> logger)
        {
            _stockAiAgentService = stockAiAgentService ?? throw new ArgumentNullException(nameof(stockAiAgentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask([FromBody] AskRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { response = "Lütfen bir mesaj girin." });
            }

            try
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                var username = HttpContext.Session.GetString("Username");
                var userId = int.TryParse(HttpContext.Session.GetString("UserId"), out var parsedUserId)
                    ? parsedUserId
                    : (int?)null;
                var responseText = await _stockAiAgentService.ProcessAsync(request.Message, userRole, userId, username, cancellationToken);
                return Json(new { response = responseText });
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning(ex, "Gemini kotası aşıldı.");
                return StatusCode(429, new { response = "Gemini kotası dolu. 1 dakika sonra tekrar deneyin veya API kotanızı artırın." });
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized || ex.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning(ex, "Gemini kimlik doğrulama/yetki hatası.");
                return StatusCode(502, new { response = "Gemini API anahtarı geçersiz veya yetkisiz. Anahtarı ve proje izinlerini kontrol edin." });
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning(ex, "Gemini model bulunamadı.");
                return StatusCode(502, new { response = "Seçili Gemini modeli kullanılamıyor. Model adını güncelleyin." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StockAI yanıt üretirken hata oluştu.");
                return StatusCode(500, new { response = "Şu anda StockAI servisine ulaşılamıyor. Lütfen tekrar deneyin." });
            }
        }

        public sealed class AskRequest
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}
