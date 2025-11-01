using Hangfire;
using PeShop.Services.Interfaces;
namespace PeShop.Configurations
{
    public static class HangfireJobRegistry
    {
        public static void RegisterRecurringJobs(this IServiceProvider provider)
        {
            // Dọn token hết hạn mỗi 10 phút
            // RecurringJob.AddOrUpdate<AuthService>(
            //     "clear-expired-tokens",
            //     service => service.ClearExpiredTokens(),
            //     "*/10 * * * *");

            // Đồng bộ dữ liệu người dùng mỗi ngày 0h
            RecurringJob.AddOrUpdate<ISQLPureService>(
                "sort-products",
                service => service.SortProductsAsync(),
                Cron.Daily);
        }
    }
}
