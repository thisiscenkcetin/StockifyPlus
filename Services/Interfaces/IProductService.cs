using StockifyPlus.Models;

namespace StockifyPlus.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllActiveProductsAsync();

        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);

        Task<Product> GetProductByIdAsync(int id);

        Task<Product> GetProductBySkuAsync(string sku);

        Task<Product> CreateProductAsync(int categoryId, string name, string sku, string description, decimal price, int criticalLevel, int initialStock);

        Task UpdateProductAsync(int id, int categoryId, string name, string sku, string description, decimal price, int criticalLevel);

        Task DeactivateProductAsync(int id);

        Task<IEnumerable<Product>> GetLowStockProductsAsync();

        Task<bool> SkuExistsAsync(string sku, int? excludeId = null);
    }
}
