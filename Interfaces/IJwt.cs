

using PeShop.Dtos.Common;
using System.Security.Claims;

namespace PeShop.Interfaces.Jwt
{
    public interface IJwt
    {
        string GenerateToken(JwtPayloadDto payloadDto);
        ClaimsPrincipal? ValidateToken(string token);
        string? GetUserIdFromToken(string token);
        List<string> GetRolesFromToken(string token);
        string? GetShopIdFromToken(string token);
        bool IsTokenValid(string token);
    }
}
