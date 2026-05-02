using StockifyPlus.Data;
using StockifyPlus.Models;
using StockifyPlus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace StockifyPlus.Services.Implementations
{
    public class StockAiAuditService : IStockAiAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StockAiAuditService> _logger;

        public StockAiAuditService(ApplicationDbContext context, ILogger<StockAiAuditService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task RecordAsync(
            int? userId,
            string? username,
            string actionType,
            string status,
            string? entityType,
            int? entityId,
            string? entityKey,
            string userPrompt,
            string agentResponse,
            string? metadata = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var log = new StockAiActionLog
                {
                    UserId = userId,
                    Username = Truncate(username, 100),
                    ActionType = TruncateRequired(actionType, 60),
                    Status = TruncateRequired(status, 40),
                    EntityType = Truncate(entityType, 80),
                    EntityId = entityId,
                    EntityKey = Truncate(entityKey, 80),
                    UserPrompt = TruncateRequired(userPrompt, 1200),
                    AgentResponse = TruncateRequired(agentResponse, 1600),
                    Metadata = Truncate(metadata, 2000),
                    CreatedAt = DateTime.Now
                };

                await _context.StockAiActionLogs.AddAsync(log, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "StockAI audit log yazılamadı. ActionType: {ActionType}", actionType);
            }
        }

        public async Task<IReadOnlyList<StockAiActionLog>> GetRecentAsync(
            int? userId,
            bool includeAllUsers,
            int count = 10,
            CancellationToken cancellationToken = default)
        {
            var takeCount = Math.Clamp(count, 1, 25);
            var query = _context.StockAiActionLogs.AsNoTracking();

            if (!includeAllUsers && userId.HasValue)
            {
                query = query.Where(log => log.UserId == userId.Value);
            }

            if (!includeAllUsers && !userId.HasValue)
            {
                return Array.Empty<StockAiActionLog>();
            }

            return await query
                .OrderByDescending(log => log.CreatedAt)
                .Take(takeCount)
                .ToListAsync(cancellationToken);
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Length <= maxLength ? value : value[..maxLength];
        }

        private static string TruncateRequired(string? value, int maxLength)
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
            return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
        }
    }
}
