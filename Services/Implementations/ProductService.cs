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

        public async Task<PagedResult<Product>> GetPagedProductsAsync(int page = 1, int pageSize = 20, string? searchTerm = null, int? categoryId = null)
        {
            if (page < 1)
                page = 1;

            if (pageSize < 1 || pageSize > 100)
                pageSize = 20;

            var query = _unitOfWork.ProductRepository
                .IncludeProperties(p => p.Category)
                .Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    p.SKU.ToLower().Contains(searchLower));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = items,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            if (categoryId <= 0)
                throw new ValidationException("Kategori ID geçerli olmalıdır.");

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
                throw new ValidationException("Ürün ID geçerli olmalıdır.");

            var product = await _unitOfWork.ProductRepository.IncludeProperties(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new NotFoundException(nameof(Product), id);

            return product;
        }

        public async Task<Product> GetProductBySkuAsync(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ValidationException("SKU boş bırakılamaz.");

            var product = await _unitOfWork.ProductRepository.IncludeProperties(p => p.Category)
                .FirstOrDefaultAsync(p => p.SKU.ToLower() == sku.ToLower());

            if (product == null)
                throw new NotFoundException($"SKU '{sku}' ile ürün bulunamadı.");

            return product;
        }

        public async Task<Product> CreateProductAsync(int categoryId, string name, string sku, string description, decimal price, int criticalLevel, int initialStock)
        {
            if (categoryId <= 0)
                throw new ValidationException("Kategori ID geçerli olmalıdır.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Ürün adı boş bırakılamaz.");

            if (string.IsNullOrWhiteSpace(sku))
                throw new ValidationException("SKU boş bırakılamaz.");

            if (price < 0)
                throw new ValidationException("Fiyat negatif olamaz.");

            if (criticalLevel < 0)
                throw new ValidationException("Kritik seviye negatif olamaz.");

            if (initialStock < 0)
                throw new ValidationException("Stok miktarı negatif olamaz.");

            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new NotFoundException(nameof(Category), categoryId);

            var skuExists = await SkuExistsAsync(sku);
            if (skuExists)
                throw new BusinessException("Bu SKU zaten kullanılıyor.");

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
                throw new ValidationException("Ürün ID geçerli olmalıdır.");

            if (categoryId <= 0)
                throw new ValidationException("Kategori ID geçerli olmalıdır.");

            var product = await GetProductByIdAsync(id);

            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new NotFoundException(nameof(Category), categoryId);

            var skuExists = await SkuExistsAsync(sku, excludeId: id);
            if (skuExists)
                throw new BusinessException("Bu SKU başka bir ürün tarafından kullanılıyor.");

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
