using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using StockifyPlus.Services.Interfaces;

namespace StockifyPlus.Controllers
{
    public class StockAIController : Controller
    {
        private readonly IGeminiApiService _geminiApiService;
        private readonly IProductService _productService;
        private readonly ILogger<StockAIController> _logger;

        public StockAIController(
            IGeminiApiService geminiApiService,
            IProductService productService,
            ILogger<StockAIController> logger)
        {
            _geminiApiService = geminiApiService ?? throw new ArgumentNullException(nameof(geminiApiService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
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
                var activeProducts = (await _productService.GetAllActiveProductsAsync()).ToList();

                var normalizedMessage = request.Message.Trim().ToLowerInvariant();

                if (IsInventoryListQuery(normalizedMessage))
                {
                    return Json(new { response = BuildInventoryListResponse(activeProducts) });
                }

                if (IsLowStockQuery(normalizedMessage))
                {
                    return Json(new { response = BuildLowStockResponse(activeProducts) });
                }

                var enrichedPrompt = BuildInventoryGroundedPrompt(request.Message, activeProducts);
                var responseText = await _geminiApiService.GenerateResponseAsync(enrichedPrompt, cancellationToken);
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
                return StatusCode(500, new { response = "�?u anda StockAI servisine ulaşılamıyor. Lütfen tekrar deneyin." });
            }
        }

        public sealed class AskRequest
        {
            public string Message { get; set; } = string.Empty;
        }

        private static bool IsInventoryListQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("depoda") &&
                (normalizedMessage.Contains("ürün") || normalizedMessage.Contains("stok")) &&
                (normalizedMessage.Contains("neler") || normalizedMessage.Contains("hangi") || normalizedMessage.Contains("liste"));
        }

        private static bool IsLowStockQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("düşük stok") ||
                normalizedMessage.Contains("kritik") ||
                normalizedMessage.Contains("azalan stok");
        }

        private static string BuildInventoryListResponse(List<Models.Product> products)
        {
            if (products.Count == 0)
            {
                return "Depoda kayıtlı aktif ürün bulunamadı.";
            }

            var lines = products
                .OrderByDescending(p => p.StockQuantity)
                .Take(20)
                .Select(p => $"- {p.Name} | Stok: {p.StockQuantity} | SKU: {p.SKU} | Kategori: {p.Category?.Name ?? "-"}")
                .ToList();

            var header = $"Depoda toplam {products.Count} aktif ürün var. İlk 20 ürün:";
            return header + Environment.NewLine + string.Join(Environment.NewLine, lines);
        }

        private static string BuildLowStockResponse(List<Models.Product> products)
        {
            var lowStockProducts = products
                .Where(p => p.StockQuantity <= p.CriticalStockLevel)
                .OrderBy(p => p.StockQuantity)
                .ToList();

            if (lowStockProducts.Count == 0)
            {
                return "Kritik stok seviyesinin altında ürün bulunmuyor.";
            }

            var lines = lowStockProducts
                .Take(20)
                .Select(p => $"- {p.Name} | Mevcut: {p.StockQuantity} | Kritik Seviye: {p.CriticalStockLevel} | SKU: {p.SKU}")
                .ToList();

            var header = $"Kritik seviyede {lowStockProducts.Count} ürün var:";
            return header + Environment.NewLine + string.Join(Environment.NewLine, lines);
        }

        private static string BuildInventoryGroundedPrompt(string userMessage, List<Models.Product> products)
        {
            var snapshot = new StringBuilder();
            snapshot.AppendLine("[CANLI_STOK_VERISI]");
            snapshot.AppendLine($"ToplamAktifUrun={products.Count}");

            foreach (var product in products.OrderBy(p => p.Name).Take(80))
            {
                snapshot.AppendLine($"Urun={product.Name};SKU={product.SKU};Kategori={product.Category?.Name ?? "-"};Stok={product.StockQuantity};Kritik={product.CriticalStockLevel};Fiyat={product.Price}");
            }

            snapshot.AppendLine("[/CANLI_STOK_VERISI]");
            snapshot.AppendLine();
            snapshot.AppendLine("KURAL: Sadece yukarıdaki canlı stok verisine dayanarak cevap ver. Veri dışında ürün/kategori uydurma.");
            snapshot.AppendLine("KURAL: Aranan bilgi canlı stok verisinde yoksa 'Bu bilgi veritabanında bulunmuyor.' de.");
            snapshot.AppendLine();
            snapshot.AppendLine("Kullanıcı Sorusu:");
            snapshot.AppendLine(userMessage.Trim());

            return snapshot.ToString();
        }
    }
}

