using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using StockifyPlus.Configurations;
using StockifyPlus.Models.Gemini;
using StockifyPlus.Services.Interfaces;

namespace StockifyPlus.Services.Implementations
{
    public class GeminiApiService : IGeminiApiService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly HttpClient _httpClient;
        private readonly GeminiOptions _options;
        private readonly ILogger<GeminiApiService> _logger;
        private const string GroqDefaultModel = "llama-3.1-8b-instant";

        public GeminiApiService(
            HttpClient httpClient,
            IOptions<GeminiOptions> options,
            ILogger<GeminiApiService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GenerateResponseAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                throw new ArgumentException("KullanÄ±cÄ± mesajÄ± boÅŸ olamaz.", nameof(userMessage));
            }

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                _logger.LogWarning("Gemini API key yapÄ±landÄ±rÄ±lmamÄ±ÅŸ. Groq fallback denenecek.");
                var groqOnlyResponse = await TryGenerateWithGroqAsync(userMessage, cancellationToken);
                if (!string.IsNullOrWhiteSpace(groqOnlyResponse))
                {
                    return groqOnlyResponse;
                }

                throw new InvalidOperationException("Gemini API yapÄ±landÄ±rmasÄ± eksik ve Groq fallback kullanÄ±lamadÄ±.");
            }

            var requestPayload = new GeminiGenerateContentRequest
            {
                SystemInstruction = new GeminiContent
                {
                    Parts = new List<GeminiPart>
                    {
                        new() { Text = _options.SystemPrompt }
                    }
                },
                Contents = new List<GeminiContent>
                {
                    new()
                    {
                        Parts = new List<GeminiPart>
                        {
                            new() { Text = userMessage.Trim() }
                        }
                    }
                }
            };

            try
            {
                var jsonBody = JsonSerializer.Serialize(requestPayload, JsonOptions);
                var modelCandidates = BuildModelCandidates(_options.Model);
                string responseBody = string.Empty;
                HttpStatusCode lastStatusCode = HttpStatusCode.OK;
                bool hasResponse = false;

                foreach (var model in modelCandidates)
                {
                    var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={Uri.EscapeDataString(_options.ApiKey)}";
                    using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

                    responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    hasResponse = true;

                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }

                    lastStatusCode = response.StatusCode;

                    var isModelNotFound = response.StatusCode == HttpStatusCode.NotFound &&
                        responseBody.Contains("not found", StringComparison.OrdinalIgnoreCase);

                    if (isModelNotFound)
                    {
                        _logger.LogWarning("Gemini model bulunamadÄ±, fallback denenecek. Model: {Model}, Body: {Body}", model, responseBody);
                        continue;
                    }

                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        _logger.LogWarning("Gemini kotasÄ±/rate limit aÅŸÄ±ldÄ±. Groq fallback denenecek. Body: {Body}", responseBody);
                        var groqResponse = await TryGenerateWithGroqAsync(userMessage, cancellationToken);
                        if (!string.IsNullOrWhiteSpace(groqResponse))
                        {
                            return groqResponse;
                        }
                    }

                    _logger.LogError("Gemini API hata dÃ¶ndÃ¼rdÃ¼. Status: {StatusCode}, Body: {Body}", response.StatusCode, responseBody);
                    throw new HttpRequestException("Gemini API isteÄŸi baÅŸarÄ±sÄ±z oldu.", null, response.StatusCode);
                }

                if (!hasResponse)
                {
                    throw new HttpRequestException("Gemini API isteÄŸi iÃ§in model bulunamadÄ±.", null, HttpStatusCode.NotFound);
                }

                if (lastStatusCode != HttpStatusCode.OK && responseBody.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError("Gemini API model fallback denemeleri baÅŸarÄ±sÄ±z oldu. Son yanÄ±t: {Body}", responseBody);
                    throw new HttpRequestException("Gemini modeli kullanÄ±lamÄ±yor. LÃ¼tfen model ayarÄ±nÄ± gÃ¼ncelleyin.", null, HttpStatusCode.NotFound);
                }

                var parsed = JsonSerializer.Deserialize<GeminiGenerateContentResponse>(responseBody, JsonOptions);

                var text = parsed?.Candidates?
                    .FirstOrDefault()?
                    .Content?
                    .Parts?
                    .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Text))?
                    .Text?
                    .Trim();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }

                if (!string.IsNullOrWhiteSpace(parsed?.PromptFeedback?.BlockReason))
                {
                    return "ÃœzgÃ¼nÃ¼m, bu iÃ§erik iÃ§in yanÄ±t Ã¼retemedim.";
                }

                _logger.LogWarning("Gemini API boÅŸ/parse edilemeyen yanÄ±t dÃ¶ndÃ¼rdÃ¼. Body: {Body}", responseBody);
                return "Åu anda uygun bir yanÄ±t Ã¼retemedim. LÃ¼tfen tekrar deneyin.";
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Gemini API isteÄŸi zaman aÅŸÄ±mÄ±na uÄŸradÄ± veya iptal edildi.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini API ile iletiÅŸim sÄ±rasÄ±nda hata oluÅŸtu.");
                throw;
            }
        }

        private async Task<string?> TryGenerateWithGroqAsync(string userMessage, CancellationToken cancellationToken)
        {
            var groqApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
            if (string.IsNullOrWhiteSpace(groqApiKey))
            {
                _logger.LogWarning("Groq fallback atlandÄ±: GROQ_API_KEY bulunamadÄ±.");
                return null;
            }

            var payload = new
            {
                model = GroqDefaultModel,
                temperature = 0.2,
                messages = new object[]
                {
                    new { role = "system", content = _options.SystemPrompt },
                    new { role = "user", content = userMessage.Trim() }
                }
            };

            var endpoint = "https://api.groq.com/openai/v1/chat/completions";
            var jsonBody = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", groqApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Groq fallback baÅŸarÄ±sÄ±z. Status: {StatusCode}, Body: {Body}", response.StatusCode, responseBody);
                return null;
            }

            using var document = JsonDocument.Parse(responseBody);
            if (!document.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                return null;
            }

            var firstChoice = choices[0];
            if (!firstChoice.TryGetProperty("message", out var messageElement))
            {
                return null;
            }

            if (!messageElement.TryGetProperty("content", out var contentElement))
            {
                return null;
            }

            var content = contentElement.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            _logger.LogInformation("YanÄ±t Groq fallback ile Ã¼retildi.");
            return content;
        }

        private static List<string> BuildModelCandidates(string configuredModel)
        {
            var candidates = new List<string>();

            if (!string.IsNullOrWhiteSpace(configuredModel))
            {
                candidates.Add(configuredModel.Trim());
            }

            candidates.Add("gemini-2.0-flash");
            candidates.Add("gemini-2.5-flash");

            return candidates
                .Where(model => !string.IsNullOrWhiteSpace(model))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
