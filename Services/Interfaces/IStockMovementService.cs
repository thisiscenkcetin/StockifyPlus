using StockifyPlus.Models;
using StockifyPlus.Models.Enums;

namespace StockifyPlus.Services.Interfaces
{
    public interface IStockMovementService
    {
        Task<IEnumerable<StockMovement>> GetAllMovementsAsync();

        Task<IEnumerable<StockMovement>> GetMovementsByProductAsync(int productId);

        Task<IEnumerable<StockMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate);

        Task<StockMovement> RecordStockInAsync(int productId, int quantity, string description);

        Task<StockMovement> RecordStockOutAsync(int productId, int quantity, string description);

        Task<StockMovement> RecordStockTransferAsync(int productId, int quantity, string description);

        Task<StockMovement> RecordStockAdjustmentAsync(int productId, int quantity, string description);

        Task ReverseMovementAsync(int movementId);

        Task<int> GetCurrentStockAsync(int productId);
    }
}
