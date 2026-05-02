namespace StockifyPlus.Services.Interfaces
{
    public interface IStockAiAgentService
    {
        Task<string> ProcessAsync(string message, string? userRole, int? userId, string? username, CancellationToken cancellationToken = default);
    }
}
