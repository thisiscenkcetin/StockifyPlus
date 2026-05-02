using Microsoft.Extensions.Caching.Distributed;
using StockifyPlus.Services.Interfaces;
using System.Text.Json;

namespace StockifyPlus.Services.Implementations
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        public RedisCacheService(
            IDistributedCache cache,
            ILogger<RedisCacheService> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key boş olamaz.", nameof(key));

            try
            {
                var cachedData = await _cache.GetStringAsync(key, cancellationToken);

                if (string.IsNullOrWhiteSpace(cachedData))
                {
                    _logger.LogDebug("Cache miss: {Key}", key);
                    return null;
                }

                _logger.LogDebug("Cache hit: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedData, JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache okuma hatası: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key boş olamaz.", nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            try
            {
                var serializedData = JsonSerializer.Serialize(value, JsonOptions);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
                };

                await _cache.SetStringAsync(key, serializedData, options, cancellationToken);
                _logger.LogDebug("Cache yazıldı: {Key}, Expiration: {Expiration}", key, expiration ?? DefaultExpiration);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache yazma hatası: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key boş olamaz.", nameof(key));

            try
            {
                await _cache.RemoveAsync(key, cancellationToken);
                _logger.LogDebug("Cache silindi: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache silme hatası: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                throw new ArgumentException("Pattern boş olamaz.", nameof(pattern));

            try
            {
                _logger.LogWarning("Pattern silme IDistributedCache ile desteklenmiyor: {Pattern}", pattern);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Pattern silme hatası: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Cache key boş olamaz.", nameof(key));

            try
            {
                var cachedData = await _cache.GetStringAsync(key, cancellationToken);
                return !string.IsNullOrWhiteSpace(cachedData);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache exists kontrolü hatası: {Key}", key);
                return false;
            }
        }

        public async Task ClearAllAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogWarning("Cache temizleme IDistributedCache ile desteklenmiyor.");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache temizleme hatası");
            }
        }
    }
}
