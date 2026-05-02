using StockifyPlus.Exceptions;
using StockifyPlus.Helpers;
using StockifyPlus.Models;
using StockifyPlus.Models.Enums;
using StockifyPlus.Repositories.Interfaces;
using StockifyPlus.Services.Interfaces;

namespace StockifyPlus.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IUnitOfWork unitOfWork, ILogger<AccountService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AppUser> RegisterAsync(string username, string password, string fullName, string email, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ValidationException("Kullanıcı adı boş bırakılamaz.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ValidationException("Şifre boş bırakılamaz.");

            var passwordErrors = PasswordHasher.GetPasswordValidationErrors(password);
            if (passwordErrors.Any())
                throw new ValidationException($"Şifre politikasına uygun değil: {string.Join(" ", passwordErrors)}");

            var exists = await UsernameExistsAsync(username);
            if (exists)
                throw new BusinessException("Bu kullanıcı adı zaten kullanılıyor.");

            var user = new AppUser
            {
                Username = username.Trim(),
                PasswordHash = PasswordHasher.HashPassword(password),
                FullName = fullName?.Trim(),
                Email = email?.Trim(),
                Role = role,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            await _unitOfWork.AppUserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return user;
        }

        public async Task<AppUser> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ValidationException("Kullanıcı adı boş bırakılamaz.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ValidationException("Şifre boş bırakılamaz.");

            var user = await _unitOfWork.AppUserRepository.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                throw new NotFoundException("Kullanıcı bulunamadı.");

            if (!user.IsActive)
                throw new BusinessException("Bu hesap deakif hale getirilmiştir.");

            if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
                throw new BusinessException("Kullanıcı adı veya şifre hatalı.");

            if (!PasswordHasher.IsBcryptHash(user.PasswordHash))
            {
                user.PasswordHash = PasswordHasher.HashPassword(password);
                _unitOfWork.AppUserRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();
            }

            try
            {
                await UpdateLastLoginAsync(user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Last login date could not be updated for user {UserId}. Login will continue.", user.Id);
            }

            return user;
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            if (id <= 0)
                throw new ValidationException("Kullanıcı ID geçerli olmalıdır.");

            var user = await _unitOfWork.AppUserRepository.GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException(nameof(AppUser), id);

            return user;
        }

        public async Task<IEnumerable<AppUser>> GetAllActiveUsersAsync()
        {
            return await _unitOfWork.AppUserRepository.FindAsync(u => u.IsActive);
        }

        public async Task UpdateUserAsync(int id, string fullName, string email, UserRole role)
        {
            var user = await GetUserByIdAsync(id);

            user.FullName = fullName?.Trim();
            user.Email = email?.Trim();
            user.Role = role;
            user.LastModifiedDate = DateTime.Now;

            _unitOfWork.AppUserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(int id, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword))
                throw new ValidationException("Eski şifre boş bırakılamaz.");

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ValidationException("Yeni şifre boş bırakılamaz.");

            var passwordErrors = PasswordHasher.GetPasswordValidationErrors(newPassword);
            if (passwordErrors.Any())
                throw new ValidationException($"Yeni şifre politikasına uygun değil: {string.Join(" ", passwordErrors)}");

            var user = await GetUserByIdAsync(id);

            if (!PasswordHasher.VerifyPassword(oldPassword, user.PasswordHash))
                throw new BusinessException("Eski şifre hatalı.");

            if (PasswordHasher.VerifyPassword(newPassword, user.PasswordHash))
                throw new BusinessException("Yeni şifre eski şifre ile aynı olamaz.");

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            user.LastModifiedDate = DateTime.Now;

            _unitOfWork.AppUserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeactivateUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);

            user.IsActive = false;
            user.LastModifiedDate = DateTime.Now;

            _unitOfWork.AppUserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            var query = await _unitOfWork.AppUserRepository
                .FindAsync(u => u.Username.ToLower() == username.ToLower());

            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);

            return query.Any();
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            user.LastLoginDate = DateTime.Now;

            _unitOfWork.AppUserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public bool IsStrongPassword(string password)
        {
            return PasswordHasher.IsPasswordStrong(password);
        }
    }
}
