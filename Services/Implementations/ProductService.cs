using StockifyPlus.Exceptions;
using StockifyPlus.Models;
using StockifyPlus.Repositories.Interfaces;
using StockifyPlus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace StockifyPlus.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IEnumerable<Product>> GetAllActiveProductsAsync()
        {
            var products = await _unitOfWork.ProductRepository.IncludeProperties(p => p.Category)
                .Where(p => p.IsActive).ToListAsync();
            return products;
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            if (categoryId <= 0)
                throw new ValidationException("Kategori ID geÃ§erli olmalÄ±dÄ±r.");

            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new NotFoundException(nameof(Category), categoryId);

            var products = await _unitOfWork.ProductRepository.IncludeProperties(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsActive).ToListAsync();
            return products;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("ÃœrÃ¼n ID geÃ§erli olmalÄ±dÄ±r.");

            var product = await _unitOfWork.ProductRepository.IncludeProperties(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new NotFoundException(nameof(Product), id);

            return product;
        }

        public async Task<Product> GetProductBySkuAsync(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ValidationException("SKU boÅŸ bÄ±rakÄ±lamaz.");

            var product = await _unitOfWork.ProductRepository.IncludeProperties(p => p.Category)
                .FirstOrDefaultAsync(p => p.SKU.ToLower() == sku.ToLower());

            if (product == null)
                throw new NotFoundException($"SKU '{sku}' ile Ã¼rÃ¼n bulunamadÄ±.");

            return product;
        }

        public async Task<Product> CreateProductAsync(int categoryId, string name, string sku, string description, decimal price, int criticalLevel, int initialStock)
        {
            if (categoryId <= 0)
                throw new ValidationException("Kategori ID geÃ§erli olmalÄ±dÄ±r.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("ÃœrÃ¼n adÄ± boÅŸ bÄ±rakÄ±lamaz.");

            if (string.IsNullOrWhiteSpace(sku))
                throw new ValidationException("SKU boÅŸ bÄ±rakÄ±lamaz.");

            if (price < 0)
                throw new ValidationException("Fiyat negatif olamaz.");

            if (criticalLevel < 0)
                throw new ValidationException("Kritik seviye negatif olamaz.");

            if (initialStock < 0)
                throw new ValidationException("Stok miktarÄ± negatif olamaz.");

            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new NotFoundException(nameof(Category), categoryId);

            var skuExists = await SkuExistsAsync(sku);
            if (skuExists)
                throw new BusinessException("Bu SKU zaten kullanÄ±lÄ±yor.");

            var product = new Product
            {
                CategoryId = categoryId,
                Name = name.Trim(),
                SKU = sku.Trim().ToUpper(),
                Description = description?.Trim(),
                Price = price,
                StockQuantity = initialStock,
                CriticalStockLevel = criticalLevel,
                IsActive = true
            };

            await _unitOfWork.ProductRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return product;
        }

        public async Task UpdateProductAsync(int id, int categoryId, string name, string sku, string description, decimal price, int criticalLevel)
        {
            if (id <= 0)
                throw new ValidationException("ÃœrÃ¼n ID geÃ§erli olmalÄ±dÄ±r.");

            if (categoryId <= 0)
                throw new ValidationException("Kategori ID geÃ§erli olmalÄ±dÄ±r.");

            var product = await GetProductByIdAsync(id);

            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new NotFoundException(nameof(Category), categoryId);

            var skuExists = await SkuExistsAsync(sku, excludeId: id);
            if (skuExists)
                throw new BusinessException("Bu SKU baÅŸka bir Ã¼rÃ¼n tarafÄ±ndan kullanÄ±lÄ±yor.");

            product.CategoryId = categoryId;
            product.Name = name.Trim();
            product.SKU = sku.Trim().ToUpper();
            product.Description = description?.Trim();
            product.Price = price;
            product.CriticalStockLevel = criticalLevel;

            _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeactivateProductAsync(int id)
        {
            var product = await GetProductByIdAsync(id);

            product.IsActive = false;
            _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
        {
            var products = await _unitOfWork.ProductRepository.IncludeProperties(p => p.Category)
                .Where(p => p.IsActive && p.StockQuantity <= p.CriticalStockLevel).ToListAsync();
            return products;
        }

        public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return false;

            var query = await _unitOfWork.ProductRepository
                .FindAsync(p => p.SKU.ToLower() == sku.ToLower());

            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);

            return query.Any();
        }
    }
}
