using Microsoft.EntityFrameworkCore;
using PeShop.Data.Contexts;
using PeShop.Data.Repositories;
using PeShop.Interfaces;
using PeShop.Services;
using PeShop.Setting;
using StackExchange.Redis;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using PeShop.Extensions;
using PeShop.Configurations;
using PeShop.Helpers;

namespace PeShop.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        string connectionString,
        IConfiguration configuration,
        IWebHostEnvironment environment
        )
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
                .FromAssembliesOf(typeof(IJwtHelper))
                .AddClasses(c => c.Where(type => type.Name.EndsWith("Helper")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Auto đăng ký Utilities
            services.Scan(scan => scan
                .FromAssembliesOf(typeof(IRedisUtil))
                .AddClasses(c => c.Where(type => type.Name.EndsWith("Util")))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            // Đăng ký AutoMapper
            services.AddAutoMapper(typeof(Program));

            // Đăng ký SmtpSettings
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.AddSingleton<SmtpSettings>(provider =>
                configuration.GetSection("SmtpSettings").Get<SmtpSettings>() ?? new SmtpSettings());





            if (environment.IsProduction())
            {

                // Đăng ký Hangfire
                services.AddHangfireWithMySql(configuration.GetConnectionString("HangfireConnectionProduction"));

                // Đăng ký Redis
                services.AddSingleton<IConnectionMultiplexer>(provider =>
                {
                    var connectionString = configuration.GetConnectionString("RedisProduct");
                    return ConnectionMultiplexer.Connect(connectionString);
                });
                // Đăng ký AppSetting
                services.Configure<AppSetting>(configuration.GetSection("AppSettingProduction"));
                services.AddSingleton<AppSetting>(provider =>
                    configuration.GetSection("AppSettingProduction").Get<AppSetting>() ?? new AppSetting());
            }
            else
            {
                // Đăng ký Hangfire
                services.AddHangfireWithMySql(configuration.GetConnectionString("HangfireConnectionLocal"));

                // Đăng ký Redis
                services.AddSingleton<IConnectionMultiplexer>(provider =>
                {
                    var connectionString = configuration.GetConnectionString("Redis");
                    return ConnectionMultiplexer.Connect(connectionString);
                });
                // Đăng ký AppSetting
                services.Configure<AppSetting>(configuration.GetSection("AppSetting"));
                services.AddSingleton<AppSetting>(provider =>
                    configuration.GetSection("AppSetting").Get<AppSetting>() ?? new AppSetting());
            }


            // Đăng ký HttpClient
            services.AddHttpClient();

            // Đăng ký ApiHelper
            services.AddScoped<IApiHelper, ApiHelper>();

            //Cors
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy
                        .WithOrigins("http://localhost:3000") // domain FE của bạn
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()); // Cho phép cookie/token
            });
            return services;
        }
    }
}
