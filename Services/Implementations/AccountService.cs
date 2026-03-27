using System.Security.Cryptography;
using System.Text;
using StockifyPlus.Exceptions;
using StockifyPlus.Models;
using StockifyPlus.Models.Enums;
using StockifyPlus.Repositories.Interfaces;
using StockifyPlus.Services.Interfaces;

namespace StockifyPlus.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<AppUser> RegisterAsync(string username, string password, string fullName, string email, UserRole role)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ValidationException("Kullanıcı adı boş bırakılamaz.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ValidationException("Şifre boş bırakılamaz.");

            if (!IsStrongPassword(password))
                throw new ValidationException("Şifre en az 8 karakter, büyük harf, küçük harf ve rakam içermeli.");

            var exists = await UsernameExistsAsync(username);
            if (exists)
                throw new BusinessException("Bu kullanıcı adı zaten kullanılıyor.");

            var user = new AppUser
            {
                Username = username.Trim(),
                PasswordHash = HashPassword(password),
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

            if (!VerifyPassword(password, user.PasswordHash))
                throw new BusinessException("Kullanıcı adı veya şifre hatalı.");

            // Son giriş zamanını güncelle
            await UpdateLastLoginAsync(user.Id);

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

            if (!IsStrongPassword(newPassword))
                throw new ValidationException("Yeni şifre en az 8 karakter, büyük harf, küçük harf ve rakam içermeli.");

            var user = await GetUserByIdAsync(id);

            if (!VerifyPassword(oldPassword, user.PasswordHash))
                throw new BusinessException("Eski şifre hatalı.");

            if (VerifyPassword(newPassword, user.PasswordHash))
                throw new BusinessException("Yeni şifre eski şifre ile aynı olamaz.");

            user.PasswordHash = HashPassword(newPassword);
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
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            bool hasUpperCase = password.Any(c => char.IsUpper(c));
            bool hasLowerCase = password.Any(c => char.IsLower(c));
            bool hasDigit = password.Any(c => char.IsDigit(c));

            return hasUpperCase && hasLowerCase && hasDigit;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }
}
