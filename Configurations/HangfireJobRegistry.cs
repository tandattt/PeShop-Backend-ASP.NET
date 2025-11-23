using Hangfire;
using PeShop.Services.Interfaces;
namespace PeShop.Configurations
{
    public static class HangfireJobRegistry
    {
        public static void RegisterRecurringJobs(this IServiceProvider provider)
        {
            var recurringJobManager = provider.GetRequiredService<IRecurringJobManager>();
            
            // Dọn token hết hạn mỗi 10 phút
            // recurringJobManager.AddOrUpdate<AuthService>(
            //     "clear-expired-tokens",
            //     service => service.ClearExpiredTokens(),
            //     "*/10 * * * *");

            // Sắp xếp sản phẩm mỗi ngày 0h
            recurringJobManager.AddOrUpdate<ISQLPureService>(
                "sort-products",
                service => service.SortProductsAsync(),
                Cron.Daily);
            recurringJobManager.AddOrUpdate<IJobService>(
                "check-products",
                service => service.ApproveProductJobAsync(),
                "*/30 * * * *");
            recurringJobManager.AddOrUpdate<IJobService>(
                "reload-cache-flask",
                service => service.ReloadCacheFlaskAsync(),
                "*/35 * * * *");
        }
    }
}
