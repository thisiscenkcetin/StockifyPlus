using Microsoft.EntityFrameworkCore;
using StockifyPlus.Data;
using StockifyPlus.Models;
using StockifyPlus.Services.Interfaces;

namespace StockifyPlus.Services.Implementations
{
    public class WishlistService : IWishlistService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WishlistService> _logger;

        public WishlistService(ApplicationDbContext context, ILogger<WishlistService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Wishlist>> GetUserWishlistAsync(int userId)
        {
            return await _context.Wishlists
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.Priority)
                .ThenByDescending(w => w.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Wishlist>> GetActiveWishlistAsync(int userId)
        {
            return await _context.Wishlists
                .Where(w => w.UserId == userId && !w.IsPurchased)
                .OrderByDescending(w => w.Priority)
                .ThenByDescending(w => w.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Wishlist>> GetWishlistByPriorityAsync(int userId, int priority)
        {
            return await _context.Wishlists
                .Where(w => w.UserId == userId && w.Priority == priority && !w.IsPurchased)
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();
        }

        public async Task<Wishlist?> GetByIdAsync(int id)
        {
            return await _context.Wishlists
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Wishlist> CreateAsync(Wishlist wishlist)
        {
            wishlist.CreatedDate = DateTime.Now;
            wishlist.IsNotified = false;
            wishlist.IsPurchased = false;

            await _context.Wishlists.AddAsync(wishlist);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Yeni wishlist öğesi oluşturuldu: {ProductName} (Kullanıcı: {UserId})", 
                wishlist.ProductName, wishlist.UserId);

            return wishlist;
        }

        public async Task<Wishlist> UpdateAsync(Wishlist wishlist)
        {
            wishlist.LastUpdatedDate = DateTime.Now;

            _context.Wishlists.Update(wishlist);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Wishlist öğesi güncellendi: {ProductName} (ID: {Id})", 
                wishlist.ProductName, wishlist.Id);

            return wishlist;
        }

        public async Task DeleteAsync(int id)
        {
            var wishlist = await _context.Wishlists.FindAsync(id);
            if (wishlist != null)
            {
                _context.Wishlists.Remove(wishlist);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Wishlist öğesi silindi: {ProductName} (ID: {Id})", 
                    wishlist.ProductName, id);
            }
        }

        public async Task MarkAsPurchasedAsync(int id)
        {
            var wishlist = await _context.Wishlists.FindAsync(id);
            if (wishlist != null)
            {
                wishlist.IsPurchased = true;
                wishlist.PurchaseDate = DateTime.Now;
                wishlist.LastUpdatedDate = DateTime.Now;

                _context.Wishlists.Update(wishlist);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Wishlist öğesi satın alındı olarak işaretlendi: {ProductName} (ID: {Id})", 
                    wishlist.ProductName, id);
            }
        }

        public async Task<IEnumerable<Wishlist>> GetItemsReachedTargetPriceAsync()
        {
            return await _context.Wishlists
                .Where(w => !w.IsPurchased 
                    && !w.IsNotified 
                    && w.TargetPrice.HasValue 
                    && w.CurrentPrice.HasValue 
                    && w.CurrentPrice <= w.TargetPrice)
                .Include(w => w.User)
                .ToListAsync();
        }

        public async Task UpdateCurrentPriceAsync(int id, decimal currentPrice)
        {
            var wishlist = await _context.Wishlists.FindAsync(id);
            if (wishlist != null)
            {
                wishlist.CurrentPrice = currentPrice;
                wishlist.LastUpdatedDate = DateTime.Now;

                if (wishlist.TargetPrice.HasValue && currentPrice <= wishlist.TargetPrice.Value)
                {
                    wishlist.IsNotified = true;
                    _logger.LogInformation("Wishlist öğesi hedef fiyata ulaştı: {ProductName} (Hedef: {TargetPrice}, Mevcut: {CurrentPrice})", 
                        wishlist.ProductName, wishlist.TargetPrice, currentPrice);
                }

                _context.Wishlists.Update(wishlist);
                await _context.SaveChangesAsync();
            }
        }
    }
}
