using StockifyPlus.Repositories.Interfaces;
using StockifyPlus.Repositories.Implementations;
using StockifyPlus.Services.Interfaces;
using StockifyPlus.Services.Implementations;
using StockifyPlus.Configurations;

namespace StockifyPlus.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IStockMovementService, StockMovementService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<INotificationSettingService, NotificationSettingService>();
            services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
            services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
            services.AddHttpClient<IGeminiApiService, GeminiApiService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }
    }
}
