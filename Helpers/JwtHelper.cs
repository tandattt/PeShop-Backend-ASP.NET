using Microsoft.IdentityModel.Tokens;
using PeShop.Interfaces;
using PeShop.Dtos.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PeShop.Helpers
{
    public class JwtHelper : IJwtHelper
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Jwt:SecretKey"] ?? throw new ArgumentNullException("Jwt:SecretKey");
        }

        /// <summary>
        /// Tạo JWT token
        /// </summary>
        /// <param name="payloadDto">Thông tin payload</param>
        /// <returns>JWT token string</returns>
        public string GenerateToken(JwtPayloadDto payloadDto)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, payloadDto.Sub),
                new Claim("token_type", payloadDto.TokenType ?? "access"),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Thêm roles/authorities
            if(payloadDto.Authorities != null){
                
            claims.AddRange(payloadDto.Authorities.Select(role => new Claim("authorities", role)));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24), // 24 giờ
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        /// <summary>
        /// Validate JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>ClaimsPrincipal nếu hợp lệ, null nếu không hợp lệ</returns>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            try
            {
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // Không cho phép trễ thời gian
                };

                var principal = tokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lấy user ID từ token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID</returns>
        public string? GetUserIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        }

        /// <summary>
        /// Lấy roles từ token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Danh sách roles</returns>
        public List<string> GetRolesFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null) return new List<string>();

            return principal.Claims
                .Where(c => c.Type == "authorities")
                .Select(c => c.Value)
                .ToList();
        }

        /// <summary>
        /// Lấy shop ID từ token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Shop ID</returns>
        public string? GetShopIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            return principal?.FindFirst("shop_id")?.Value;
        }

        /// <summary>
        /// Kiểm tra token có hết hạn không
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>True nếu token hợp lệ và chưa hết hạn</returns>
        public bool IsTokenValid(string token)
        {
            return ValidateToken(token) != null;
        }
    }
}