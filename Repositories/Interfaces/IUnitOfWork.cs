using StockifyPlus.Models;

namespace StockifyPlus.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Category> CategoryRepository { get; }

        IRepository<Product> ProductRepository { get; }

        IRepository<StockMovement> StockMovementRepository { get; }

        IRepository<AppUser> AppUserRepository { get; }

        Task<int> SaveChangesAsync();

        int SaveChanges();

        Task RollbackAsync();
    }
}
