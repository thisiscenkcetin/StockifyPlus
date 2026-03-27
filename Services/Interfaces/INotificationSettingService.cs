using StockifyPlus.Models;

namespace StockifyPlus.Services.Interfaces
{
    public interface INotificationSettingService
    {
        Task<NotificationSetting> GetOrCreateAsync(CancellationToken cancellationToken = default);
        Task UpdateAsync(bool pushEnabled, string alertEmail, string updatedBy, CancellationToken cancellationToken = default);
        Task<bool> IsPushEnabledAsync(CancellationToken cancellationToken = default);
        Task<string> GetAlertEmailAsync(CancellationToken cancellationToken = default);
    }
}
