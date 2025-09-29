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

namespace PeShop.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, string connectionString, IConfiguration configuration)
        {
            // Đăng ký DbContext
            services.AddDbContext<PeShopDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            // Đăng ký JWT Service
            services.AddScoped<IJwt, JwtUtil>();

            // Đăng ký Repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // Đăng ký Services
            services.AddScoped<IAuthService, AuthService>();

            // Đăng ký Redis Service
            services.AddScoped<IRedisUtil, RedisUtil>();
            
            // Đăng ký Mail Service
            services.AddScoped<IMailService, MailService>();

            // Đăng ký SmtpSettings
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.AddSingleton<SmtpSettings>(provider => 
                configuration.GetSection("SmtpSettings").Get<SmtpSettings>() ?? new SmtpSettings());

            // Đăng ký AppSetting
            services.Configure<AppSetting>(configuration.GetSection("AppSetting"));
            services.AddSingleton<AppSetting>(provider => 
                configuration.GetSection("AppSetting").Get<AppSetting>() ?? new AppSetting());

            // Đăng ký Email Service
            services.AddScoped<IEmailUtil, EmailUtil>();

            // Đăng ký Redis
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var connectionString = configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(connectionString);
            });

            // services.AddScoped<IDatabase>(provider =>
            // {
            //     var connectionMultiplexer = provider.GetRequiredService<IConnectionMultiplexer>();
            //     return connectionMultiplexer.GetDatabase();
            // });

            // AutoMapper
            // services.AddAutoMapper(typeof(Program));

            // FluentValidation
            // services.AddValidatorsFromAssemblyContaining<Program>();

            return services;
        }
    }
}
