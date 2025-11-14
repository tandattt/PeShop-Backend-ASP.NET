using PeShop.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using PeShop.Filters;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
    options.Filters.Add<SuccessResponseFilter>();
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfig(builder.Environment);

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
    });

builder.Services.AddApplicationServices(builder.Configuration.GetConnectionString("DefaultConnection"), builder.Configuration, builder.Environment);

var app = builder.Build();
app.UseRateLimiter();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}
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
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers().RequireRateLimiting("ip-policy");

app.Run();
