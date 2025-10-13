using Hangfire;
using Hangfire.MySql;
using Microsoft.Extensions.DependencyInjection;

namespace PeShop.Configurations
{
    public static class HangfireConfiguration
    {
        public static IServiceCollection AddHangfireWithMySql(this IServiceCollection services, string connectionString)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseStorage(new MySqlStorage(connectionString, new MySqlStorageOptions
                {
                    // TablePrefix = "Hangfire_",
                    PrepareSchemaIfNecessary = true // ✅ Tự tạo bảng nếu chưa có
                })));

            services.AddHangfireServer();
            return services;
        }
    }
}
