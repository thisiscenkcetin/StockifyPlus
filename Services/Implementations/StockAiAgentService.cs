using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using StockifyPlus.Exceptions;
using StockifyPlus.Models;
using StockifyPlus.Models.Enums;
using StockifyPlus.Services.Interfaces;

namespace StockifyPlus.Services.Implementations
{
    public class StockAiAgentService : IStockAiAgentService
    {
        private static readonly Regex KeyValueRegex = new(
            @"(?<key>ad|isim|name|sku|kategori|category|fiyat|price|kritik|kritikstok|critical|stok|stock|miktar|quantity|qty|açıklama|aciklama|description)\s*[:=]\s*(?<value>.*?)(?=\s+(?:ad|isim|name|sku|kategori|category|fiyat|price|kritik|kritikstok|critical|stok|stock|miktar|quantity|qty|açıklama|aciklama|description)\s*[:=]|$)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly IGeminiApiService _geminiApiService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IStockMovementService _stockMovementService;
        private readonly IStockAiAuditService _stockAiAuditService;

        public StockAiAgentService(
            IGeminiApiService geminiApiService,
            IProductService productService,
            ICategoryService categoryService,
            IStockMovementService stockMovementService,
            IStockAiAuditService stockAiAuditService)
        {
            _geminiApiService = geminiApiService ?? throw new ArgumentNullException(nameof(geminiApiService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _stockMovementService = stockMovementService ?? throw new ArgumentNullException(nameof(stockMovementService));
            _stockAiAuditService = stockAiAuditService ?? throw new ArgumentNullException(nameof(stockAiAuditService));
        }

        public async Task<string> ProcessAsync(string message, string? userRole, int? userId, string? username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "Lütfen bir mesaj girin.";
            }

            var activeProducts = (await _productService.GetAllActiveProductsAsync()).ToList();
            var normalizedMessage = Normalize(message);

            if (IsHelpQuery(normalizedMessage))
            {
                return BuildHelpResponse(userRole);
            }

            if (IsPhaseStatusQuery(normalizedMessage))
            {
                return BuildPhaseStatusResponse();
            }

            if (IsStockMovementIntent(normalizedMessage))
            {
                return await HandleStockMovementAsync(message, userRole, userId, username, cancellationToken);
            }

            if (IsUpdateProductIntent(normalizedMessage))
            {
                return await HandleUpdateProductAsync(message, userRole, userId, username, cancellationToken);
            }

            if (IsCreateProductIntent(normalizedMessage))
            {
                return await HandleCreateProductAsync(message, userRole, userId, username, cancellationToken);
            }

            if (IsInventoryListQuery(normalizedMessage))
            {
                var response = BuildInventoryListResponse(activeProducts);
                await RecordInsightAsync(userId, username, "InventoryList", message, response, cancellationToken);
                return response;
            }

            if (IsRiskAnalysisQuery(normalizedMessage))
            {
                var response = BuildRiskAnalysisResponse(activeProducts, userRole);
                await RecordInsightAsync(userId, username, "RiskAnalysis", message, response, cancellationToken);
                return response;
            }

            if (IsLowStockQuery(normalizedMessage))
            {
                var response = BuildLowStockResponse(activeProducts);
                await RecordInsightAsync(userId, username, "LowStockAnalysis", message, response, cancellationToken);
                return response;
            }

            if (IsCategoryHealthQuery(normalizedMessage))
            {
                var response = BuildCategoryHealthResponse(activeProducts);
                await RecordInsightAsync(userId, username, "CategoryHealth", message, response, cancellationToken);
                return response;
            }

            if (IsAuditHistoryQuery(normalizedMessage))
            {
                return await BuildAuditHistoryResponseAsync(userRole, userId, cancellationToken);
            }

            if (IsMemorySummaryQuery(normalizedMessage))
            {
                return await BuildMemorySummaryResponseAsync(userRole, userId, cancellationToken);
            }

            if (IsUndoSuggestionQuery(normalizedMessage))
            {
                return await BuildUndoSuggestionResponseAsync(userRole, userId, cancellationToken);
            }

            if (IsReorderRecommendationQuery(normalizedMessage))
            {
                var response = BuildReorderRecommendationResponse(activeProducts, userRole);
                await RecordInsightAsync(userId, username, "ReorderRecommendation", message, response, cancellationToken);
                return response;
            }

            if (IsOperationalGuidanceQuery(normalizedMessage))
            {
                var response = BuildOperationalGuidanceResponse(activeProducts, userRole);
                await RecordInsightAsync(userId, username, "OperationalGuidance", message, response, cancellationToken);
                return response;
            }

            var enrichedPrompt = BuildInventoryGroundedPrompt(message, activeProducts);
            return await _geminiApiService.GenerateResponseAsync(enrichedPrompt, cancellationToken);
        }

        private async Task<string> HandleCreateProductAsync(string message, string? userRole, int? userId, string? username, CancellationToken cancellationToken)
        {
            if (!CanWriteProducts(userRole))
            {
                return "Bu işlem için ürün yazma yetkisi gerekiyor. Analiz yapabilirim ama ürün kaydı oluşturamam.";
            }

            var fields = ParseFields(message);
            var validation = ValidateCreateFields(fields);
            if (!string.IsNullOrWhiteSpace(validation))
            {
                return validation;
            }

            var categories = (await _categoryService.GetAllActiveCategoriesAsync()).ToList();
            var category = FindCategory(categories, fields["kategori"]);
            if (category == null)
            {
                return BuildCategoryNotFoundResponse(fields["kategori"], categories);
            }

            var name = fields["ad"];
            var sku = fields["sku"].ToUpperInvariant();
            var description = fields.TryGetValue("açıklama", out var parsedDescription) ? parsedDescription : string.Empty;
            var price = ParseDecimal(fields["fiyat"]);
            var critical = ParseInt(fields["kritik"]);
            var stock = ParseInt(fields["stok"]);

            if (!HasApproval(message))
            {
                var previewResponse = string.Join(Environment.NewLine, new[]
                {
                    "Ürün ekleme önizlemesi hazır. Kaydetmem için aynı komuta `onayla` ekle.",
                    $"- Ad: {name}",
                    $"- SKU: {sku}",
                    $"- Kategori: {category.Name}",
                    $"- Fiyat: {price:0.##}",
                    $"- Kritik stok: {critical}",
                    $"- İlk stok: {stock}",
                    string.IsNullOrWhiteSpace(description) ? "- Açıklama: -" : $"- Açıklama: {description}"
                });
                await RecordAuditAsync(userId, username, "ProductCreate", "Preview", "Product", null, sku, message, previewResponse, $"Category={category.Name};InitialStock={stock}", cancellationToken);
                return previewResponse;
            }

            if (await _productService.SkuExistsAsync(sku))
            {
                return $"`{sku}` SKU değerine sahip bir ürün zaten var. Yeni ürün için benzersiz SKU kullan.";
            }

            var product = await _productService.CreateProductAsync(
                category.Id,
                name,
                sku,
                description,
                price,
                critical,
                stock);

            var response = $"Ürün oluşturuldu: {product.Name} | SKU: {product.SKU} | Stok: {product.StockQuantity} | Kategori: {category.Name}";
            await RecordAuditAsync(userId, username, "ProductCreate", "Applied", "Product", product.Id, product.SKU, message, response, $"Category={category.Name};InitialStock={stock}", cancellationToken);
            return response;
        }

        private async Task<string> HandleUpdateProductAsync(string message, string? userRole, int? userId, string? username, CancellationToken cancellationToken)
        {
            if (!CanWriteProducts(userRole))
            {
                return "Bu işlem için ürün düzenleme yetkisi gerekiyor. Analiz yapabilirim ama ürün kaydını değiştiremem.";
            }

            var fields = ParseFields(message);
            if (!fields.TryGetValue("sku", out var sku) || string.IsNullOrWhiteSpace(sku))
            {
                return "Ürün düzenlemek için SKU belirtmelisin. Örnek: `ürün güncelle sku=PLA-001 fiyat=475 kritik=8 onayla`";
            }

            Product product;
            try
            {
                product = await _productService.GetProductBySkuAsync(sku);
            }
            catch (NotFoundException)
            {
                return $"`{sku}` SKU değerine sahip ürün bulunamadı.";
            }

            var categories = (await _categoryService.GetAllActiveCategoriesAsync()).ToList();
            var category = product.Category;
            if (fields.TryGetValue("kategori", out var categoryName) && !string.IsNullOrWhiteSpace(categoryName))
            {
                category = FindCategory(categories, categoryName);
                if (category == null)
                {
                    return BuildCategoryNotFoundResponse(categoryName, categories);
                }
            }

            var nextName = fields.TryGetValue("ad", out var name) ? name : product.Name;
            var nextDescription = fields.TryGetValue("açıklama", out var description) ? description : product.Description;
            var nextPrice = fields.TryGetValue("fiyat", out var priceText) ? ParseDecimal(priceText) : product.Price;
            var nextCritical = fields.TryGetValue("kritik", out var criticalText) ? ParseInt(criticalText) : product.CriticalStockLevel;

            if (nextPrice < 0)
            {
                return "Fiyat negatif olamaz.";
            }

            if (nextCritical < 0)
            {
                return "Kritik stok negatif olamaz.";
            }

            if (!HasApproval(message))
            {
                var previewResponse = string.Join(Environment.NewLine, new[]
                {
                    "Ürün düzenleme önizlemesi hazır. Uygulamam için aynı komuta `onayla` ekle.",
                    $"- Ürün: {product.Name} ({product.SKU})",
                    $"- Yeni ad: {nextName}",
                    $"- Kategori: {category?.Name ?? "-"}",
                    $"- Fiyat: {product.Price:0.##} -> {nextPrice:0.##}",
                    $"- Kritik stok: {product.CriticalStockLevel} -> {nextCritical}",
                    string.IsNullOrWhiteSpace(nextDescription) ? "- Açıklama: -" : $"- Açıklama: {nextDescription}"
                });
                await RecordAuditAsync(userId, username, "ProductUpdate", "Preview", "Product", product.Id, product.SKU, message, previewResponse, $"Category={category?.Name ?? "-"};Price={product.Price:0.##}->{nextPrice:0.##};Critical={product.CriticalStockLevel}->{nextCritical}", cancellationToken);
                return previewResponse;
            }

            if (category == null)
            {
                return "Ürünün kategorisi bulunamadı. Önce geçerli bir kategori belirt.";
            }

            await _productService.UpdateProductAsync(
                product.Id,
                category.Id,
                nextName,
                product.SKU,
                nextDescription ?? string.Empty,
                nextPrice,
                nextCritical);

            var response = $"Ürün güncellendi: {nextName} | SKU: {product.SKU} | Fiyat: {nextPrice:0.##} | Kritik stok: {nextCritical}";
            await RecordAuditAsync(userId, username, "ProductUpdate", "Applied", "Product", product.Id, product.SKU, message, response, $"Category={category.Name};Price={product.Price:0.##}->{nextPrice:0.##};Critical={product.CriticalStockLevel}->{nextCritical}", cancellationToken);
            return response;
        }

        private async Task<string> HandleStockMovementAsync(string message, string? userRole, int? userId, string? username, CancellationToken cancellationToken)
        {
            if (!CanWriteStock(userRole))
            {
                return "Bu işlem için stok hareketi yetkisi gerekiyor. Analiz yapabilirim ama stok miktarını değiştiremem.";
            }

            var fields = ParseFields(message);
            if (!fields.TryGetValue("sku", out var sku) || string.IsNullOrWhiteSpace(sku))
            {
                return "Stok hareketi için SKU belirtmelisin. Örnek: `stok giriş sku=PLA-001 miktar=10 açıklama=Tedarik onayla`";
            }

            if (!fields.TryGetValue("miktar", out var quantityText) && !fields.TryGetValue("stok", out quantityText))
            {
                return "Stok hareketi için miktar belirtmelisin. Örnek: `stok çıkış sku=PLA-001 miktar=3 onayla`";
            }

            var quantity = ParseInt(quantityText);
            if (quantity == 0)
            {
                return "Stok hareketi miktarı 0 olamaz.";
            }

            Product product;
            try
            {
                product = await _productService.GetProductBySkuAsync(sku);
            }
            catch (NotFoundException)
            {
                return $"`{sku}` SKU değerine sahip ürün bulunamadı.";
            }

            var movementKind = DetectStockMovementKind(message);
            if (movementKind == StockAiMovementKind.Unknown)
            {
                return "Stok hareket tipini anlayamadım. `stok giriş`, `stok çıkış` veya `stok ayarla` şeklinde yaz.";
            }

            if (movementKind is StockAiMovementKind.In or StockAiMovementKind.Out && quantity < 0)
            {
                return "Stok giriş/çıkış miktarı pozitif olmalı. Negatif düzeltme için `stok ayarla` kullan.";
            }

            var signedQuantity = movementKind switch
            {
                StockAiMovementKind.In => quantity,
                StockAiMovementKind.Out => -quantity,
                StockAiMovementKind.Adjustment => quantity,
                _ => quantity
            };

            var projectedStock = product.StockQuantity + signedQuantity;
            if (projectedStock < 0)
            {
                return $"Bu işlem stok miktarını negatife düşürür. Mevcut stok: {product.StockQuantity}, hareket: {signedQuantity}.";
            }

            var description = fields.TryGetValue("açıklama", out var parsedDescription)
                ? parsedDescription
                : "StockAI agent işlemi";

            var isRisky = IsRiskyStockMovement(product, signedQuantity, projectedStock);
            if (!HasApproval(message))
            {
                var previewResponse = BuildStockMovementPreview(product, movementKind, signedQuantity, projectedStock, description, isRisky, requiresRiskApproval: false);
                await RecordAuditAsync(userId, username, "StockMovement", "Preview", "Product", product.Id, product.SKU, message, previewResponse, $"Movement={movementKind};Quantity={signedQuantity};ProjectedStock={projectedStock};Risky={isRisky}", cancellationToken);
                return previewResponse;
            }

            if (isRisky && !HasRiskApproval(message))
            {
                var previewResponse = BuildStockMovementPreview(product, movementKind, signedQuantity, projectedStock, description, isRisky, requiresRiskApproval: true);
                await RecordAuditAsync(userId, username, "StockMovement", "Preview", "Product", product.Id, product.SKU, message, previewResponse, $"Movement={movementKind};Quantity={signedQuantity};ProjectedStock={projectedStock};Risky={isRisky};RequiresRiskApproval=True", cancellationToken);
                return previewResponse;
            }

            var agentDescription = $"StockAI: {description}".Trim();
            switch (movementKind)
            {
                case StockAiMovementKind.In:
                    await _stockMovementService.RecordStockInAsync(product.Id, Math.Abs(quantity), agentDescription);
                    break;
                case StockAiMovementKind.Out:
                    await _stockMovementService.RecordStockOutAsync(product.Id, Math.Abs(quantity), agentDescription);
                    break;
                case StockAiMovementKind.Adjustment:
                    await _stockMovementService.RecordStockAdjustmentAsync(product.Id, quantity, agentDescription);
                    break;
            }

            var riskNote = projectedStock <= product.CriticalStockLevel
                ? " Kritik stok seviyesinde veya altında; yeniden tedarik planı önerilir."
                : string.Empty;

            var response = $"Stok hareketi uygulandı: {product.Name} | SKU: {product.SKU} | Yeni stok: {projectedStock}.{riskNote}";
            await RecordAuditAsync(userId, username, "StockMovement", "Applied", "Product", product.Id, product.SKU, message, response, $"Movement={movementKind};Quantity={signedQuantity};ProjectedStock={projectedStock};Risky={isRisky}", cancellationToken);
            return response;
        }

        private static bool IsCreateProductIntent(string normalizedMessage)
        {
            return normalizedMessage.Contains("ürün") &&
                (normalizedMessage.Contains("ekle") ||
                 normalizedMessage.Contains("olustur") ||
                 normalizedMessage.Contains("oluştur") ||
                 normalizedMessage.Contains("yeni ürün") ||
                 normalizedMessage.Contains("kaydet"));
        }

        private static bool IsUpdateProductIntent(string normalizedMessage)
        {
            return normalizedMessage.Contains("ürün") &&
                (normalizedMessage.Contains("güncelle") ||
                 normalizedMessage.Contains("guncelle") ||
                 normalizedMessage.Contains("düzenle") ||
                 normalizedMessage.Contains("duzenle"));
        }

        private static bool IsInventoryListQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("depoda") &&
                (normalizedMessage.Contains("ürün") || normalizedMessage.Contains("stok")) &&
                (normalizedMessage.Contains("neler") || normalizedMessage.Contains("hangi") || normalizedMessage.Contains("liste"));
        }

        private static bool IsHelpQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("yardım") ||
                normalizedMessage.Contains("yardim") ||
                normalizedMessage.Contains("ne yapabilirsin") ||
                normalizedMessage.Contains("neler yapabilirsin") ||
                normalizedMessage.Contains("komut") ||
                normalizedMessage.Contains("nasıl kullan") ||
                normalizedMessage.Contains("nasil kullan");
        }

        private static bool IsPhaseStatusQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("hangi faz") ||
                normalizedMessage.Contains("faz durumu") ||
                normalizedMessage.Contains("proje durumu") ||
                normalizedMessage.Contains("geliştirme durumu") ||
                normalizedMessage.Contains("gelistirme durumu") ||
                normalizedMessage.Contains("neler tamamlandı") ||
                normalizedMessage.Contains("neler tamamlandi");
        }

        private static bool IsLowStockQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("düşük stok") ||
                normalizedMessage.Contains("dusuk stok") ||
                normalizedMessage.Contains("kritik") ||
                normalizedMessage.Contains("azalan stok");
        }

        private static bool IsRiskAnalysisQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("risk analizi") ||
                normalizedMessage.Contains("riskli ürün") ||
                normalizedMessage.Contains("riskli urun") ||
                normalizedMessage.Contains("en riskli") ||
                normalizedMessage.Contains("acil ürün") ||
                normalizedMessage.Contains("acil urun") ||
                normalizedMessage.Contains("öncelik analizi") ||
                normalizedMessage.Contains("oncelik analizi");
        }

        private static bool IsStockMovementIntent(string normalizedMessage)
        {
            return normalizedMessage.Contains("stok") &&
                (normalizedMessage.Contains("giris") ||
                 normalizedMessage.Contains("giriş") ||
                 normalizedMessage.Contains("cikis") ||
                 normalizedMessage.Contains("çıkış") ||
                 normalizedMessage.Contains("ayarla") ||
                 normalizedMessage.Contains("ayarlama") ||
                 normalizedMessage.Contains("dus") ||
                 normalizedMessage.Contains("düş") ||
                 normalizedMessage.Contains("artir") ||
                 normalizedMessage.Contains("artır"));
        }

        private static bool IsOperationalGuidanceQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("bugün ne yap") ||
                normalizedMessage.Contains("bugun ne yap") ||
                normalizedMessage.Contains("operasyon özeti") ||
                normalizedMessage.Contains("operasyon ozeti") ||
                normalizedMessage.Contains("görev") ||
                normalizedMessage.Contains("gorev") ||
                normalizedMessage.Contains("yönlendir") ||
                normalizedMessage.Contains("yonlendir");
        }

        private static bool IsCategoryHealthQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("kategori analizi") ||
                normalizedMessage.Contains("kategori sağlık") ||
                normalizedMessage.Contains("kategori saglik") ||
                normalizedMessage.Contains("stok sağlığı") ||
                normalizedMessage.Contains("stok sagligi") ||
                normalizedMessage.Contains("stok sağlık") ||
                normalizedMessage.Contains("stok saglik");
        }

        private static bool IsAuditHistoryQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("son ai işlem") ||
                normalizedMessage.Contains("son ai islem") ||
                normalizedMessage.Contains("ai geçmiş") ||
                normalizedMessage.Contains("ai gecmis") ||
                normalizedMessage.Contains("audit") ||
                normalizedMessage.Contains("stockai geçmiş") ||
                normalizedMessage.Contains("stockai gecmis");
        }

        private static bool IsUndoSuggestionQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("geri al") ||
                normalizedMessage.Contains("geri alma") ||
                normalizedMessage.Contains("ters işlem") ||
                normalizedMessage.Contains("ters islem") ||
                normalizedMessage.Contains("son işlemi düzelt") ||
                normalizedMessage.Contains("son islemi duzelt") ||
                normalizedMessage.Contains("iptal et");
        }

        private static bool IsMemorySummaryQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("son öner") ||
                normalizedMessage.Contains("son oner") ||
                normalizedMessage.Contains("son cevab") ||
                normalizedMessage.Contains("ne önermiştin") ||
                normalizedMessage.Contains("ne onermistin") ||
                normalizedMessage.Contains("son plan") ||
                normalizedMessage.Contains("son aksiyon") ||
                normalizedMessage.Contains("hafıza") ||
                normalizedMessage.Contains("hafiza");
        }

        private static bool IsReorderRecommendationQuery(string normalizedMessage)
        {
            return normalizedMessage.Contains("tedarik") ||
                normalizedMessage.Contains("sipariş öner") ||
                normalizedMessage.Contains("siparis oner") ||
                normalizedMessage.Contains("yeniden sipariş") ||
                normalizedMessage.Contains("yeniden siparis") ||
                normalizedMessage.Contains("satın alma") ||
                normalizedMessage.Contains("satin alma") ||
                normalizedMessage.Contains("ne kadar almalı") ||
                normalizedMessage.Contains("ne kadar almali");
        }

        private static Dictionary<string, string> ParseFields(string message)
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (Match match in KeyValueRegex.Matches(message))
            {
                var key = NormalizeFieldKey(match.Groups["key"].Value);
                var value = StripApprovalWords(match.Groups["value"].Value.Trim());

                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                {
                    fields[key] = value;
                }
            }

            return fields;
        }

        private static string NormalizeFieldKey(string key)
        {
            return Normalize(key) switch
            {
                "ad" or "isim" or "name" => "ad",
                "sku" => "sku",
                "kategori" or "category" => "kategori",
                "fiyat" or "price" => "fiyat",
                "kritik" or "kritikstok" or "critical" => "kritik",
                "stok" or "stock" => "stok",
                "miktar" or "quantity" or "qty" => "miktar",
                "açıklama" or "aciklama" or "description" => "açıklama",
                _ => string.Empty
            };
        }

        private static string? ValidateCreateFields(Dictionary<string, string> fields)
        {
            var required = new[] { "ad", "sku", "kategori", "fiyat", "kritik", "stok" };
            var missing = required.Where(field => !fields.ContainsKey(field)).ToList();

            if (missing.Count > 0)
            {
                return "Ürün eklemek için eksik alanlar var: " + string.Join(", ", missing) +
                    Environment.NewLine +
                    "Örnek: `ürün ekle ad=Iphone 14 sku=IPHONE-001 kategori=IPHONE fiyat=45000 kritik=5 stok=20 onayla`";
            }

            if (ParseDecimal(fields["fiyat"]) < 0)
            {
                return "Fiyat negatif olamaz.";
            }

            if (ParseInt(fields["kritik"]) < 0 || ParseInt(fields["stok"]) < 0)
            {
                return "Stok ve kritik stok negatif olamaz.";
            }

            return null;
        }

        private static Category? FindCategory(IEnumerable<Category> categories, string categoryName)
        {
            return categories.FirstOrDefault(category =>
                string.Equals(category.Name, categoryName.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildCategoryNotFoundResponse(string categoryName, List<Category> categories)
        {
            var categoryList = categories.Count == 0
                ? "Aktif kategori yok."
                : string.Join(", ", categories.Select(c => c.Name).OrderBy(name => name));

            return $"`{categoryName}` kategorisi bulunamadı. Mevcut kategoriler: {categoryList}";
        }

        private static bool CanWriteProducts(string? userRole)
        {
            return string.Equals(userRole, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(userRole, UserRole.DepoPersoneli.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool CanWriteStock(string? userRole)
        {
            return string.Equals(userRole, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(userRole, UserRole.DepoPersoneli.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasApproval(string message)
        {
            var normalized = Normalize(message);
            return normalized.Contains("onayla") ||
                normalized.Contains("kaydet") ||
                normalized.Contains("uygula");
        }

        private static bool HasRiskApproval(string message)
        {
            var normalized = Normalize(message);
            return normalized.Contains("risk onayla") ||
                normalized.Contains("kesin onayla") ||
                normalized.Contains("kritik onayla");
        }

        private static string StripApprovalWords(string value)
        {
            return Regex.Replace(value, @"\s+(onayla|kaydet|uygula|risk onayla|kesin onayla|kritik onayla)\s*$", string.Empty, RegexOptions.IgnoreCase).Trim();
        }

        private static decimal ParseDecimal(string value)
        {
            var normalized = value.Trim().Replace("₺", string.Empty).Trim();

            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.GetCultureInfo("tr-TR"), out var trValue))
            {
                return trValue;
            }

            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariantValue))
            {
                return invariantValue;
            }

            throw new ValidationException($"Sayısal değer okunamadı: {value}");
        }

        private static int ParseInt(string value)
        {
            if (int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.GetCultureInfo("tr-TR"), out var result))
            {
                return result;
            }

            throw new ValidationException($"Tam sayı değeri okunamadı: {value}");
        }

        private static string Normalize(string value)
        {
            return value.Trim().ToLower(CultureInfo.GetCultureInfo("tr-TR"));
        }

        private static StockAiMovementKind DetectStockMovementKind(string message)
        {
            var normalized = Normalize(message);

            if (normalized.Contains("giris") || normalized.Contains("giriş") || normalized.Contains("artir") || normalized.Contains("artır"))
            {
                return StockAiMovementKind.In;
            }

            if (normalized.Contains("cikis") || normalized.Contains("çıkış") || normalized.Contains("dus") || normalized.Contains("düş"))
            {
                return StockAiMovementKind.Out;
            }

            if (normalized.Contains("ayarla") || normalized.Contains("ayarlama"))
            {
                return StockAiMovementKind.Adjustment;
            }

            return StockAiMovementKind.Unknown;
        }

        private static bool IsRiskyStockMovement(Product product, int signedQuantity, int projectedStock)
        {
            if (signedQuantity >= 0)
            {
                return false;
            }

            var absoluteQuantity = Math.Abs(signedQuantity);
            return projectedStock <= product.CriticalStockLevel ||
                absoluteQuantity >= Math.Max(20, product.StockQuantity / 2);
        }

        private static string BuildStockMovementPreview(
            Product product,
            StockAiMovementKind movementKind,
            int signedQuantity,
            int projectedStock,
            string description,
            bool isRisky,
            bool requiresRiskApproval)
        {
            var movementLabel = movementKind switch
            {
                StockAiMovementKind.In => "Stok girişi",
                StockAiMovementKind.Out => "Stok çıkışı",
                StockAiMovementKind.Adjustment => "Stok ayarlama",
                _ => "Stok hareketi"
            };

            var approvalLine = requiresRiskApproval
                ? "Bu riskli bir stok düşüşü. Uygulamam için komuta `kesin onayla` ekle."
                : "Uygulamam için aynı komuta `onayla` ekle.";

            var riskLine = isRisky
                ? $"- Risk: Yeni stok kritik seviyeye yakın veya altında olabilir. Kritik seviye: {product.CriticalStockLevel}"
                : "- Risk: Normal";

            return string.Join(Environment.NewLine, new[]
            {
                $"{movementLabel} önizlemesi hazır. {approvalLine}",
                $"- Ürün: {product.Name} ({product.SKU})",
                $"- Mevcut stok: {product.StockQuantity}",
                $"- Hareket: {signedQuantity:+#;-#;0}",
                $"- Yeni stok: {projectedStock}",
                riskLine,
                string.IsNullOrWhiteSpace(description) ? "- Açıklama: -" : $"- Açıklama: {description}"
            });
        }

        private static string BuildInventoryListResponse(List<Product> products)
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

        private static string BuildHelpResponse(string? userRole)
        {
            var lines = new List<string>
            {
                "StockAI komut rehberi:",
                $"Rol odağı: {BuildRoleFocus(userRole)}",
                "- Analiz: `risk analizi`, `kategori analizi`, `tedarik öner`, `bugün ne yapmalıyım`",
                "- Hafıza: `son ai işlemleri`, `son önerim`, `geri al`",
                "- Listeleme: `depoda hangi ürünler var liste`"
            };

            if (CanWriteProducts(userRole))
            {
                lines.Add("- Ürün ekleme: `ürün ekle ad=Iphone 14 sku=IPHONE-001 kategori=IPHONE fiyat=45000 kritik=5 stok=20 onayla`");
                lines.Add("- Ürün güncelleme: `ürün güncelle sku=PLA-001 fiyat=475 kritik=8 onayla`");
            }
            else
            {
                lines.Add("- Ürün işlemleri: Bu rolde ürün kaydı yazmam; analiz ve öneri verebilirim.");
            }

            if (CanWriteStock(userRole))
            {
                lines.Add("- Stok girişi: `stok giriş sku=PLA-001 miktar=10 açıklama=Tedarik onayla`");
                lines.Add("- Stok çıkışı: `stok çıkış sku=PLA-001 miktar=3 onayla`");
                lines.Add("- Riskli düşüş: Kritik veya büyük düşüşlerde `kesin onayla` isterim.");
            }
            else
            {
                lines.Add("- Stok işlemleri: Bu rolde stok değiştirmem; kritik stok ve tedarik analizi yapabilirim.");
            }

            lines.Add("Güvenlik: Yazma işlemlerinde `onayla`, riskli stok düşüşlerinde ayrıca `kesin onayla` gerekir.");
            return string.Join(Environment.NewLine, lines);
        }

        private static string BuildPhaseStatusResponse()
        {
            return string.Join(Environment.NewLine, new[]
            {
                "StockAI geliştirme durumu:",
                "- Faz 1 - Agent çekirdeği: Tamamlandı.",
                "- Faz 2 - Stok tool'ları: Tamamlandı.",
                "- Faz 3 - Analiz motoru: Büyük kapsam tamamlandı; risk analizi, kategori sağlığı, tedarik önerisi ve operasyon özeti aktif.",
                "- Faz 4 - Rol bazlı agent: Büyük kapsam tamamlandı; yetki matrisi, rol odağı ve role özel aksiyonlar aktif.",
                "- Faz 5 - Hafıza ve audit: Büyük kapsam tamamlandı; audit, preview/insight hafızası, kısa hafıza ve geri alma önerisi aktif.",
                "Sıradaki ileri seviye geliştirme: LLM tool-call soyutlaması, satış hızı verisiyle tahminleme ve gerçek görev atama ekranı."
            });
        }

        private static string BuildLowStockResponse(List<Product> products)
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

        private static string BuildRiskAnalysisResponse(List<Product> products, string? userRole)
        {
            if (products.Count == 0)
            {
                return "Risk analizi için aktif ürün bulunamadı.";
            }

            var riskItems = products
                .Where(p => p.CriticalStockLevel > 0)
                .Select(p =>
                {
                    var shortage = Math.Max(0, p.CriticalStockLevel - p.StockQuantity);
                    var nearCriticalGap = Math.Max(0, p.CriticalStockLevel * 2 - p.StockQuantity);
                    var stockValue = p.Price * p.StockQuantity;
                    var score = shortage * 10 + nearCriticalGap * 3;

                    if (p.StockQuantity == 0)
                    {
                        score += 50;
                    }

                    if (stockValue >= 10000)
                    {
                        score += 8;
                    }

                    return new
                    {
                        Product = p,
                        Shortage = shortage,
                        NearCriticalGap = nearCriticalGap,
                        StockValue = stockValue,
                        Score = score
                    };
                })
                .Where(item => item.Score > 0)
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Product.StockQuantity)
                .Take(10)
                .ToList();

            if (riskItems.Count == 0)
            {
                return "Risk analizi: Kritik seviyeye yaklaşan ürün yok. Öncelik, stok doğruluğunu korumak ve yüksek stoklu ürünleri sermaye açısından izlemek.";
            }

            var lines = new List<string>
            {
                $"Risk analizi: {riskItems.Count} ürün öncelikli izlenmeli.",
                $"Rol odağı: {BuildRoleFocus(userRole)}"
            };

            foreach (var item in riskItems)
            {
                var level = item.Product.StockQuantity == 0
                    ? "Tükendi"
                    : item.Product.StockQuantity <= item.Product.CriticalStockLevel
                        ? "Kritik"
                        : "Yaklaşıyor";

                var reason = item.Shortage > 0
                    ? $"kritik seviyenin {item.Shortage} altında"
                    : $"kritik seviyeye {item.NearCriticalGap} birim mesafede";

                lines.Add($"- {level} | Skor: {item.Score} | {item.Product.Name} | SKU: {item.Product.SKU} | Stok: {item.Product.StockQuantity} | Kritik: {item.Product.CriticalStockLevel} | Neden: {reason} | Stok değeri: {item.StockValue:0.##}");
            }

            lines.Add("Önerilen aksiyon: İlk 3 ürünü fiziksel say, tedarik ihtiyacını doğrula, stok hareketi gerekiyorsa StockAI üzerinden onaylı işlem yap.");
            lines.Add(BuildRoleAction(userRole, "risk"));

            return string.Join(Environment.NewLine, lines);
        }

        private static string BuildCategoryHealthResponse(List<Product> products)
        {
            if (products.Count == 0)
            {
                return "Kategori sağlığı için aktif ürün bulunamadı.";
            }

            var categorySummaries = products
                .GroupBy(p => p.Category?.Name ?? "Kategorisiz")
                .Select(group =>
                {
                    var totalProducts = group.Count();
                    var lowStockCount = group.Count(p => p.StockQuantity <= p.CriticalStockLevel);
                    var stockValue = group.Sum(p => p.Price * p.StockQuantity);
                    var totalStock = group.Sum(p => p.StockQuantity);
                    var healthScore = totalProducts == 0
                        ? 100
                        : Math.Max(0, 100 - (int)Math.Round(lowStockCount * 100m / totalProducts));

                    return new
                    {
                        CategoryName = group.Key,
                        TotalProducts = totalProducts,
                        LowStockCount = lowStockCount,
                        TotalStock = totalStock,
                        StockValue = stockValue,
                        HealthScore = healthScore
                    };
                })
                .OrderBy(summary => summary.HealthScore)
                .ThenByDescending(summary => summary.LowStockCount)
                .Take(8)
                .ToList();

            var lines = new List<string>
            {
                "Kategori bazlı stok sağlığı:"
            };

            foreach (var summary in categorySummaries)
            {
                var status = summary.HealthScore switch
                {
                    >= 85 => "Sağlıklı",
                    >= 60 => "İzlenmeli",
                    _ => "Riskli"
                };

                lines.Add($"- {summary.CategoryName}: {status} | Skor: {summary.HealthScore}/100 | Ürün: {summary.TotalProducts} | Kritik: {summary.LowStockCount} | Stok: {summary.TotalStock} | Değer: {summary.StockValue:0.##}");
            }

            var mostRisky = categorySummaries.FirstOrDefault(summary => summary.LowStockCount > 0);
            if (mostRisky != null)
            {
                lines.Add($"Öneri: Önce `{mostRisky.CategoryName}` kategorisindeki kritik ürünleri fiziksel sayım ve tedarik açısından kontrol et.");
            }

            return string.Join(Environment.NewLine, lines);
        }

        private async Task<string> BuildAuditHistoryResponseAsync(string? userRole, int? userId, CancellationToken cancellationToken)
        {
            var includeAllUsers = string.Equals(userRole, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase);
            var logs = await _stockAiAuditService.GetRecentAsync(userId, includeAllUsers, 10, cancellationToken);

            if (logs.Count == 0)
            {
                return includeAllUsers
                    ? "Henüz StockAI tarafından uygulanmış işlem kaydı yok."
                    : "Senin kullanıcı hesabın için henüz StockAI işlem kaydı yok.";
            }

            var scopeText = includeAllUsers ? "tüm kullanıcılar" : "senin işlemlerin";
            var lines = new List<string>
            {
                $"Son StockAI işlem kayıtları ({scopeText}):"
            };

            foreach (var log in logs)
            {
                var actor = includeAllUsers ? $" | Kullanıcı: {log.Username ?? "-"}" : string.Empty;
                lines.Add($"- {log.CreatedAt:dd.MM.yyyy HH:mm} | {log.ActionType} | {log.Status} | {log.EntityKey ?? "-"}{actor}");
            }

            lines.Add("Not: `Applied` uygulanmış işlemi, `Preview` onaysız işlem taslağını, `Insight` analiz/öneri cevabını gösterir.");
            return string.Join(Environment.NewLine, lines);
        }

        private async Task<string> BuildMemorySummaryResponseAsync(string? userRole, int? userId, CancellationToken cancellationToken)
        {
            var includeAllUsers = string.Equals(userRole, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase);
            var logs = await _stockAiAuditService.GetRecentAsync(userId, includeAllUsers, 10, cancellationToken);
            var memoryItems = logs
                .Where(log =>
                    string.Equals(log.Status, "Insight", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(log.Status, "Preview", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(log.Status, "Applied", StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .ToList();

            if (memoryItems.Count == 0)
            {
                return includeAllUsers
                    ? "StockAI hafızasında özetlenecek son kayıt yok."
                    : "Senin kullanıcın için StockAI hafızasında özetlenecek son kayıt yok.";
            }

            var scopeText = includeAllUsers ? "tüm kullanıcılar" : "senin kayıtların";
            var lines = new List<string>
            {
                $"StockAI kısa hafıza özeti ({scopeText}):"
            };

            foreach (var item in memoryItems)
            {
                lines.Add($"- {item.CreatedAt:dd.MM.yyyy HH:mm} | {item.Status} | {item.ActionType} | {item.EntityKey ?? "-"}");
                lines.Add($"  İstek: {TrimForMemory(item.UserPrompt, 140)}");
                lines.Add($"  Cevap: {TrimForMemory(item.AgentResponse, 180)}");
            }

            lines.Add("Not: Analiz/öneri cevapları `Insight`, onaysız işlem taslakları `Preview`, uygulanan işlemler `Applied` olarak tutulur.");
            return string.Join(Environment.NewLine, lines);
        }

        private async Task<string> BuildUndoSuggestionResponseAsync(string? userRole, int? userId, CancellationToken cancellationToken)
        {
            var includeAllUsers = string.Equals(userRole, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase);
            var logs = await _stockAiAuditService.GetRecentAsync(userId, includeAllUsers, 5, cancellationToken);
            var lastApplied = logs.FirstOrDefault(log => string.Equals(log.Status, "Applied", StringComparison.OrdinalIgnoreCase));

            if (lastApplied == null)
            {
                return includeAllUsers
                    ? "Geri alma önerisi üretmek için uygulanmış StockAI işlemi bulunamadı."
                    : "Senin kullanıcın için geri alma önerisi üretilecek uygulanmış StockAI işlemi bulunamadı.";
            }

            var lines = new List<string>
            {
                $"Son uygulanmış işlem: {lastApplied.CreatedAt:dd.MM.yyyy HH:mm} | {lastApplied.ActionType} | {lastApplied.EntityKey ?? "-"}"
            };

            if (string.Equals(lastApplied.ActionType, "StockMovement", StringComparison.OrdinalIgnoreCase))
            {
                var quantity = ExtractMetadataInt(lastApplied.Metadata, "Quantity");
                if (quantity.HasValue && !string.IsNullOrWhiteSpace(lastApplied.EntityKey))
                {
                    var inverseQuantity = -quantity.Value;
                    var movementCommand = inverseQuantity > 0
                        ? $"stok giriş sku={lastApplied.EntityKey} miktar={inverseQuantity} açıklama=StockAI geri alma onayla"
                        : $"stok çıkış sku={lastApplied.EntityKey} miktar={Math.Abs(inverseQuantity)} açıklama=StockAI geri alma onayla";

                    lines.Add("Güvenli geri alma önerisi: Son stok hareketinin tersini yeni bir onaylı stok hareketi olarak işle.");
                    lines.Add($"Önerilen komut: `{movementCommand}`");

                    if (inverseQuantity < 0)
                    {
                        lines.Add("Not: Ters işlem stok düşüşü olduğu için kritik seviyeye yaklaştırırsa sistem ek risk onayı isteyebilir.");
                    }

                    return string.Join(Environment.NewLine, lines);
                }
            }

            if (string.Equals(lastApplied.ActionType, "ProductCreate", StringComparison.OrdinalIgnoreCase))
            {
                lines.Add("Geri alma önerisi: Ürün oluşturma işlemi otomatik silinmez. Ürüne stok hareketi bağlanmış olabilir; önce ürün detayını ve hareket geçmişini kontrol et.");
                lines.Add("Güvenli aksiyon: Gerekiyorsa ürünü manuel olarak pasife alma/silme akışından değerlendir; StockAI şu an ürün silme işlemi yapmaz.");
                return string.Join(Environment.NewLine, lines);
            }

            if (string.Equals(lastApplied.ActionType, "ProductUpdate", StringComparison.OrdinalIgnoreCase))
            {
                lines.Add("Geri alma önerisi: Ürün güncellemesi için eski/yeni alanlar audit metadata içinde kısmi tutuluyor; otomatik geri alma yerine önce değişikliği kontrol et.");
                lines.Add($"Metadata: {lastApplied.Metadata ?? "-"}");
                lines.Add("Güvenli aksiyon: Eski değeri doğruladıktan sonra `ürün güncelle sku=... ... onayla` komutuyla düzeltme yap.");
                return string.Join(Environment.NewLine, lines);
            }

            lines.Add("Bu işlem türü için otomatik geri alma önerisi tanımlı değil. Audit kaydını inceleyip manuel düzeltme yap.");
            return string.Join(Environment.NewLine, lines);
        }

        private static string BuildReorderRecommendationResponse(List<Product> products, string? userRole)
        {
            var candidates = products
                .Where(p => p.CriticalStockLevel > 0 && p.StockQuantity <= p.CriticalStockLevel * 2)
                .Select(p =>
                {
                    var shortage = Math.Max(0, p.CriticalStockLevel - p.StockQuantity);
                    var suggestedQuantity = Math.Max(p.CriticalStockLevel * 2 - p.StockQuantity, p.CriticalStockLevel);
                    var priorityScore = shortage * 3 + Math.Max(0, p.CriticalStockLevel * 2 - p.StockQuantity);

                    return new
                    {
                        Product = p,
                        Shortage = shortage,
                        SuggestedQuantity = suggestedQuantity,
                        PriorityScore = priorityScore,
                        EstimatedCost = suggestedQuantity * p.Price
                    };
                })
                .OrderByDescending(item => item.PriorityScore)
                .ThenBy(item => item.Product.StockQuantity)
                .Take(10)
                .ToList();

            if (candidates.Count == 0)
            {
                return "Tedarik önerisi için kritik seviyeye yakın ürün bulunmuyor. Şu an satın alma yerine sayım doğruluğu ve atıl stok kontrolü daha öncelikli.";
            }

            var totalEstimatedCost = candidates.Sum(item => item.EstimatedCost);
            var lines = new List<string>
            {
                $"Tedarik önerisi: {candidates.Count} ürün için tahmini alım maliyeti {totalEstimatedCost:0.##}.",
                $"Rol odağı: {BuildRoleFocus(userRole)}"
            };

            foreach (var item in candidates)
            {
                var status = item.Product.StockQuantity <= item.Product.CriticalStockLevel
                    ? "Acil"
                    : "Yaklaşıyor";

                lines.Add($"- {status} | {item.Product.Name} | SKU: {item.Product.SKU} | Stok: {item.Product.StockQuantity} | Kritik: {item.Product.CriticalStockLevel} | Önerilen alım: {item.SuggestedQuantity} | Tahmini maliyet: {item.EstimatedCost:0.##}");
            }

            lines.Add("Personel yönlendirme: Depo personeli listedeki SKU'ları fiziksel saysın, admin/yönetici tedarik miktarını onaylasın, alım sonrası stok girişi StockAI ile onaylı işlenebilir.");
            lines.Add(BuildRoleAction(userRole, "reorder"));
            lines.Add("Not: Satış hızı verisi olmadığı için öneriler mevcut stok ve kritik seviye üzerinden konservatif hesaplandı.");

            return string.Join(Environment.NewLine, lines);
        }

        private static string BuildOperationalGuidanceResponse(List<Product> products, string? userRole)
        {
            var activeCount = products.Count;
            var lowStockProducts = products
                .Where(p => p.StockQuantity <= p.CriticalStockLevel)
                .OrderBy(p => p.StockQuantity - p.CriticalStockLevel)
                .ThenBy(p => p.StockQuantity)
                .Take(5)
                .ToList();

            var overstockProducts = products
                .Where(p => p.CriticalStockLevel > 0 && p.StockQuantity >= p.CriticalStockLevel * 6)
                .OrderByDescending(p => p.StockQuantity)
                .Take(3)
                .ToList();

            var totalInventoryValue = products.Sum(p => p.Price * p.StockQuantity);
            var lines = new List<string>
            {
                $"Operasyon özeti: {activeCount} aktif ürün, tahmini stok değeri {totalInventoryValue:0.##}.",
                $"Rol odağı: {BuildRoleFocus(userRole)}"
            };

            if (lowStockProducts.Count > 0)
            {
                lines.Add("Öncelik 1 - Kritik stok kontrolü:");
                lines.AddRange(lowStockProducts.Select(p => $"- {p.Name} | SKU: {p.SKU} | Stok: {p.StockQuantity} | Kritik: {p.CriticalStockLevel}"));
            }
            else
            {
                lines.Add("Öncelik 1 - Kritik stok: Şu an kritik seviyede ürün yok.");
            }

            if (overstockProducts.Count > 0)
            {
                lines.Add("Öncelik 2 - Aşırı stok/sermaye kontrolü:");
                lines.AddRange(overstockProducts.Select(p => $"- {p.Name} | SKU: {p.SKU} | Stok: {p.StockQuantity} | Kritik: {p.CriticalStockLevel}"));
            }

            lines.Add("Önerilen aksiyon: Kritik listedeki ürünleri fiziksel say, tedarik kararını ver, sonra stok hareketlerini StockAI ile onaylı işle.");
            lines.Add(BuildRoleAction(userRole, "operations"));

            return string.Join(Environment.NewLine, lines);
        }

        private static string BuildRoleFocus(string? userRole)
        {
            if (string.Equals(userRole, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return "risk, veri kalitesi ve operasyon kararı";
            }

            if (string.Equals(userRole, UserRole.Muhasebeci.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return "stok değeri, fiyat etkisi ve raporlama";
            }

            return "sayım, stok hareketi ve kritik ürün takibi";
        }

        private static string BuildRoleAction(string? userRole, string context)
        {
            if (string.Equals(userRole, UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return context switch
                {
                    "reorder" => "Rol aksiyonu: Tedarik bütçesini ve önerilen miktarları onayla; onaydan sonra depo personeline stok girişi görevi ver.",
                    "risk" => "Rol aksiyonu: En yüksek skorlu ürünlerde tedarik, fiyat ve veri doğruluğu kararını aynı gün netleştir.",
                    _ => "Rol aksiyonu: Kritik ürünler için sorumlu personeli belirle, tedarik kararını onayla ve gün sonunda audit geçmişini kontrol et."
                };
            }

            if (string.Equals(userRole, UserRole.Muhasebeci.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return context switch
                {
                    "reorder" => "Rol aksiyonu: Tahmini alım maliyetini bütçe ile karşılaştır, fiyat sapması olan SKU'ları admin onayına çıkar.",
                    "risk" => "Rol aksiyonu: Stok değeri yüksek ve kritik seviyeye yakın ürünleri maliyet etkisi açısından raporla.",
                    _ => "Rol aksiyonu: Kritik ve aşırı stok listelerini nakit akışı, stok değeri ve fiyat etkisi açısından değerlendir."
                };
            }

            return context switch
            {
                "reorder" => "Rol aksiyonu: Listedeki SKU'ları fiziksel say, sayım farkı varsa stok ayarlama komutunu onaylı şekilde işle.",
                "risk" => "Rol aksiyonu: İlk 3 riskli SKU için raf kontrolü yap, eksik veya hatalı kayıt varsa stok hareketini onaylı uygula.",
                _ => "Rol aksiyonu: Kritik listedeki ürünleri say, raf yerini doğrula ve gerekli stok giriş/çıkışlarını StockAI ile onaylı işle."
            };
        }

        private static int? ExtractMetadataInt(string? metadata, string key)
        {
            if (string.IsNullOrWhiteSpace(metadata))
            {
                return null;
            }

            var match = Regex.Match(metadata, $@"(?:^|;){Regex.Escape(key)}=(?<value>-?\d+)(?:;|$)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!match.Success)
            {
                return null;
            }

            return int.TryParse(match.Groups["value"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : null;
        }

        private static string TrimForMemory(string value, int maxLength)
        {
            var normalized = Regex.Replace(value, @"\s+", " ").Trim();
            return normalized.Length <= maxLength
                ? normalized
                : normalized[..Math.Max(0, maxLength - 3)] + "...";
        }

        private static string BuildInventoryGroundedPrompt(string userMessage, List<Product> products)
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
            snapshot.AppendLine("KURAL: Kullanıcı operasyonel aksiyon istiyorsa uygulanabilir, kısa ve güvenli adımlar ver.");
            snapshot.AppendLine();
            snapshot.AppendLine("Kullanıcı Sorusu:");
            snapshot.AppendLine(userMessage.Trim());

            return snapshot.ToString();
        }

        private async Task RecordAuditAsync(
            int? userId,
            string? username,
            string actionType,
            string status,
            string? entityType,
            int? entityId,
            string? entityKey,
            string userPrompt,
            string agentResponse,
            string? metadata,
            CancellationToken cancellationToken)
        {
            await _stockAiAuditService.RecordAsync(
                userId,
                username,
                actionType,
                status,
                entityType,
                entityId,
                entityKey,
                userPrompt,
                agentResponse,
                metadata,
                cancellationToken);
        }

        private Task RecordInsightAsync(
            int? userId,
            string? username,
            string actionType,
            string userPrompt,
            string agentResponse,
            CancellationToken cancellationToken)
        {
            return RecordAuditAsync(
                userId,
                username,
                actionType,
                "Insight",
                null,
                null,
                null,
                userPrompt,
                agentResponse,
                "Source=DeterministicAnalysis",
                cancellationToken);
        }

        private enum StockAiMovementKind
        {
            Unknown = 0,
            In = 1,
            Out = 2,
            Adjustment = 3
        }
    }
}
