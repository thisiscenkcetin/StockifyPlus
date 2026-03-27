using StockifyPlus.Models;

namespace StockifyPlus.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllActiveCategoriesAsync();

        Task<Category> GetCategoryByIdAsync(int id);

        Task<Category> CreateCategoryAsync(string name, string description = null);

        Task UpdateCategoryAsync(int id, string name, string description);

        Task DeactivateCategoryAsync(int id);

        Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null);
    }
}
