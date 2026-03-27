using StockifyPlus.Models;
using StockifyPlus.Models.Enums;

namespace StockifyPlus.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AppUser> RegisterAsync(string username, string password, string fullName, string email, UserRole role);

        Task<AppUser> LoginAsync(string username, string password);

        Task<AppUser> GetUserByIdAsync(int id);

        Task<IEnumerable<AppUser>> GetAllActiveUsersAsync();

        Task UpdateUserAsync(int id, string fullName, string email, UserRole role);

        Task ChangePasswordAsync(int id, string oldPassword, string newPassword);

        Task DeactivateUserAsync(int id);

        Task<bool> UsernameExistsAsync(string username, int? excludeId = null);

        Task UpdateLastLoginAsync(int userId);

        bool IsStrongPassword(string password);
    }
}
