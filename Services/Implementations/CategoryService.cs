using StockifyPlus.Exceptions;
using StockifyPlus.Models;
using StockifyPlus.Repositories.Interfaces;
using StockifyPlus.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace StockifyPlus.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IEnumerable<Category>> GetAllActiveCategoriesAsync()
        {
            return await _unitOfWork.CategoryRepository
                .IncludeProperties(c => c.Products)
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("Kategori ID geÃ§erli olmalÄ±dÄ±r.");

            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new NotFoundException(nameof(Category), id);

            return category;
        }

        public async Task<Category> CreateCategoryAsync(string name, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Kategori adÄ± boÅŸ bÄ±rakÄ±lamaz.");

            if (name.Length < 2 || name.Length > 100)
                throw new ValidationException("Kategori adÄ± 2-100 karakter arasÄ±nda olmalÄ±dÄ±r.");

            var exists = await CategoryNameExistsAsync(name);
            if (exists)
                throw new BusinessException("Bu kategori adÄ± zaten mevcut.");

            var category = new Category
            {
                Name = name.Trim(),
                Description = description?.Trim(),
                IsActive = true
            };

            await _unitOfWork.CategoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category;
        }

        public async Task UpdateCategoryAsync(int id, string name, string description)
        {
            if (id <= 0)
                throw new ValidationException("Kategori ID geÃ§erli olmalÄ±dÄ±r.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Kategori adÄ± boÅŸ bÄ±rakÄ±lamaz.");

            var category = await GetCategoryByIdAsync(id);

            var nameExists = await CategoryNameExistsAsync(name, excludeId: id);
            if (nameExists)
                throw new BusinessException("Bu kategori adÄ± baÅŸka bir kategori tarafÄ±ndan kullanÄ±lÄ±yor.");

            category.Name = name.Trim();
            if (!string.IsNullOrWhiteSpace(description))
                category.Description = description.Trim();

            _unitOfWork.CategoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeactivateCategoryAsync(int id)
        {
            var category = await GetCategoryByIdAsync(id);

            var activeProducts = await _unitOfWork.ProductRepository
                .FindAsync(p => p.CategoryId == id && p.IsActive);

            if (activeProducts.Any())
                throw new BusinessException("Bu kategoriyle iliÅŸkili aktif Ã¼rÃ¼nler var. Ã–nce Ã¼rÃ¼nleri deaktif hale getirin.");

            category.IsActive = false;
            _unitOfWork.CategoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var query = await _unitOfWork.CategoryRepository
                .FindAsync(c => c.Name.ToLower() == name.ToLower() && c.IsActive);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return query.Any();
        }
    }
}
