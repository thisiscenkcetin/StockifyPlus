using StockifyPlus.Models;

namespace StockifyPlus.Services.Interfaces
{
    public interface IWishlistService
    {
        Task<IEnumerable<Wishlist>> GetUserWishlistAsync(int userId);

        Task<IEnumerable<Wishlist>> GetActiveWishlistAsync(int userId);

        Task<IEnumerable<Wishlist>> GetWishlistByPriorityAsync(int userId, int priority);

        Task<Wishlist?> GetByIdAsync(int id);

        Task<Wishlist> CreateAsync(Wishlist wishlist);

        Task<Wishlist> UpdateAsync(Wishlist wishlist);

        Task DeleteAsync(int id);

        Task MarkAsPurchasedAsync(int id);

        Task<IEnumerable<Wishlist>> GetItemsReachedTargetPriceAsync();

        Task UpdateCurrentPriceAsync(int id, decimal currentPrice);
    }
}
