using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Data.Repositories;
using PeShop.Interfaces.Jwt;
using PeShop.Services;
using PeShop.Utilities;
using PeShop.Helpers;
using PeShop.Interfaces;
using Microsoft.Extensions.Configuration;
using PeShop.Setting;
using StackExchange.Redis;
using PeShop.Services.Interfaces;
using Scrutor;
namespace PeShop.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, string connectionString, IConfiguration configuration)
        {
            // Đăng ký DbContext
            services.AddDbContext<PeShopDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // Auto đăng ký Services
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(IAuthService))
                .AddClasses(c => c.Where(type => type.Name.EndsWith("Service")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Auto đăng ký Repositories
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(IUserRepository))
                .AddClasses(c => c.Where(type => type.Name.EndsWith("Repository")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Auto đăng ký Helpers
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(IJwtHelpers))
                .AddClasses(c => c.Where(type => type.Name.EndsWith("Helper")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Auto đăng ký Utilities
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(IRedisUtil))
                .AddClasses(c => c.Where(type => type.Name.EndsWith("Util")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Đăng ký SmtpSettings
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.AddSingleton<SmtpSettings>(provider => 
                configuration.GetSection("SmtpSettings").Get<SmtpSettings>() ?? new SmtpSettings());

            // Đăng ký AppSetting
            services.Configure<AppSetting>(configuration.GetSection("AppSetting"));
            services.AddSingleton<AppSetting>(provider => 
                configuration.GetSection("AppSetting").Get<AppSetting>() ?? new AppSetting());


            // Đăng ký Redis
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var connectionString = configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(connectionString);
            });


            return services;
        }
    }
}
