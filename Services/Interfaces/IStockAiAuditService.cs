namespace StockifyPlus.Services.Interfaces
{
    using StockifyPlus.Models;

    public interface IStockAiAuditService
    {
        Task RecordAsync(
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
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<StockAiActionLog>> GetRecentAsync(
            int? userId,
            bool includeAllUsers,
            int count = 10,
            CancellationToken cancellationToken = default);
    }
}
