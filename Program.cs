using PeShop.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using PeShop.Filters;
using Hangfire;
using PeShop.Constants;
using PeShop.SignalR;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

// Add MVC support for Views (bao gồm cả API Controllers)
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
    options.Filters.Add<SuccessResponseFilter>();
})
.AddViewLocalization()
.AddDataAnnotationsLocalization()
.AddRazorOptions(options =>
{
    options.ViewLocationFormats.Add("/MVC/Views/{1}/{0}.cshtml");
    options.ViewLocationFormats.Add("/MVC/Views/Shared/{0}.cshtml");
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfig(builder.Environment);

// Add SignalR
builder.Services.AddSignalR();

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:SecretKey"] ?? throw new ArgumentNullException("Jwt:SecretKey"))),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            RoleClaimType = "authorities",
            NameClaimType = JwtRegisteredClaimNames.Sub,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // SignalR gửi token qua query ?access_token=
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                // Path đã được UsePathBase strip prefix rồi, chỉ cần check /hubs/notification
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/notification"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };

    });

builder.Services.AddApplicationServices(builder.Configuration.GetConnectionString("DefaultConnection"), builder.Configuration, builder.Environment);

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UsePathBase("/dotnet-peshop");
}

// Đếm request - đặt trước rate limiter để đếm tất cả requests (bao gồm cả bị chặn)
app.UseRequestCounter();

app.UseRateLimiter();

// Đếm request thực tế được xử lý - đặt sau rate limiter để chỉ đếm request đã vượt qua rate limit
app.UseProcessedRequestCounter();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

// 🔹 Seed permissions on startup
await PeShop.Data.Seeders.PermissionSeeder.SeedPermissionsAsync(app.Services);

// 🔹 Đăng ký job định kỳ
using (var scope = app.Services.CreateScope())
{
    var provider = scope.ServiceProvider;
    provider.RegisterRecurringJobs();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();
// .RequireRateLimiting(PolicyConstants.IpPolicy);

// Map MVC routes - tất cả routes MVC nằm dưới /manage
app.MapControllerRoute(
    name: "manage",
    pattern: "manage/{controller=GHNTool}/{action=SwitchStatus}/{id?}");

// Redirect root đến /manage (sẽ redirect đến GHN Tool)
app.MapGet("/", () => Results.Redirect("/manage/ghn-tool/switch-status"));

app.MapHub<NotificationHub>("/hubs/notification");

app.Run();
