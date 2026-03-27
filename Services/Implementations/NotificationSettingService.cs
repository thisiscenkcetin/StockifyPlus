using Microsoft.EntityFrameworkCore;
using StockifyPlus.Data;
using StockifyPlus.Models;
using StockifyPlus.Services.Interfaces;

namespace StockifyPlus.Services.Implementations
{
    public class NotificationSettingService : INotificationSettingService
    {
        private readonly ApplicationDbContext _dbContext;

        public NotificationSettingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<NotificationSetting> GetOrCreateAsync(CancellationToken cancellationToken = default)
        {
            var setting = await _dbContext.NotificationSettings
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (setting != null)
            {
                return setting;
            }

            setting = new NotificationSetting
            {
                PushEnabled = true,
                AlertEmail = string.Empty,
                LastUpdatedBy = "System",
                LastUpdatedAt = DateTime.Now
            };

            await _dbContext.NotificationSettings.AddAsync(setting, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return setting;
        }

        public async Task UpdateAsync(bool pushEnabled, string alertEmail, string updatedBy, CancellationToken cancellationToken = default)
        {
            var setting = await GetOrCreateAsync(cancellationToken);
            setting.PushEnabled = pushEnabled;
            setting.AlertEmail = (alertEmail ?? string.Empty).Trim();
            setting.LastUpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "System" : updatedBy.Trim();
            setting.LastUpdatedAt = DateTime.Now;

            _dbContext.NotificationSettings.Update(setting);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> IsPushEnabledAsync(CancellationToken cancellationToken = default)
        {
            var setting = await GetOrCreateAsync(cancellationToken);
            return setting.PushEnabled;
        }

        public async Task<string> GetAlertEmailAsync(CancellationToken cancellationToken = default)
        {
            var setting = await GetOrCreateAsync(cancellationToken);
            return setting.AlertEmail;
        }
    }
}
