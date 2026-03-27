using StockifyPlus.Data;
using StockifyPlus.Models;
using StockifyPlus.Repositories.Interfaces;

namespace StockifyPlus.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<Category> _categoryRepository;
        private IRepository<Product> _productRepository;
        private IRepository<StockMovement> _stockMovementRepository;
        private IRepository<AppUser> _appUserRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IRepository<Category> CategoryRepository
        {
            get
            {
                if (_categoryRepository == null)
                    _categoryRepository = new Repository<Category>(_context);
                return _categoryRepository;
            }
        }

        public IRepository<Product> ProductRepository
        {
            get
            {
                if (_productRepository == null)
                    _productRepository = new Repository<Product>(_context);
                return _productRepository;
            }
        }

        public IRepository<StockMovement> StockMovementRepository
        {
            get
            {
                if (_stockMovementRepository == null)
                    _stockMovementRepository = new Repository<StockMovement>(_context);
                return _stockMovementRepository;
            }
        }

        public IRepository<AppUser> AppUserRepository
        {
            get
            {
                if (_appUserRepository == null)
                    _appUserRepository = new Repository<AppUser>(_context);
                return _appUserRepository;
            }
        }


        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }


        public async Task RollbackAsync()
        {
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case Microsoft.EntityFrameworkCore.EntityState.Added:
                        entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                        break;
                    case Microsoft.EntityFrameworkCore.EntityState.Modified:
                    case Microsoft.EntityFrameworkCore.EntityState.Deleted:
                        entry.Reload();
                        break;
                }
            }
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
