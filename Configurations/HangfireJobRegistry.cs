using Hangfire;
using PeShop.Services;

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
            // RecurringJob.AddOrUpdate<UserService>(
            //     "sync-user-stats",
            //     service => service.SyncUserStatistics(),
            //     Cron.Daily);
        }
    }
}
