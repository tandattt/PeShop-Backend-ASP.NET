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
using PeShop.Middleware;
using MySqlConnector;
using PeShop.GlobalVariables;
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
            {
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySqlOptions =>
                {
                    // Disable retry strategy to allow manual transactions
                    mySqlOptions.CommandTimeout(30);
                });
                options.EnableSensitiveDataLogging(environment.IsDevelopment());
                options.EnableDetailedErrors(environment.IsDevelopment());
            });

            int warmCount = WarmConnection.WarmConnectionCount; // số connection muốn tạo sẵn

            for (int i = 0; i < warmCount; i++)
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                // ngay sau khi using kết thúc -> connection trả vào pool
            }
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

            // Đăng ký VnPaySetting
            services.Configure<VnPaySetting>(configuration.GetSection("VnPay"));
            services.AddSingleton<VnPaySetting>(provider =>
                configuration.GetSection("VnPay").Get<VnPaySetting>() ?? new VnPaySetting());




            // Đăng ký Hangfire
            services.AddHangfireWithMySql(configuration.GetConnectionString("HangfireConnection"));

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


            // Đăng ký HttpClient
            services.AddHttpClient();

            // Đăng ký ApiHelper
            services.AddScoped<IApiHelper, ApiHelper>();

            //Cors - Mở full, không giới hạn domain
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy
                        // .WithOrigins("http://localhost:3000","http://169.254.47.240:5500","http://127.0.0.1:5500") // domain FE của bạn
                        .SetIsOriginAllowed(_ => true) // Cho phép tất cả origins
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()); // Cho phép cookie/token
            });
            // Rate Limiting
            services.AddRateLimiterPolicies();
            return services;
        }
    }
}
