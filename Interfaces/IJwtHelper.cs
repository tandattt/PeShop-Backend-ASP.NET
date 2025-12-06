

using PeShop.Dtos.Common;
using System.Security.Claims;

namespace PeShop.Interfaces
{
    public interface IJwtHelper
    {
        string GenerateToken(JwtPayloadDto payloadDto);
        ClaimsPrincipal? ValidateToken(string token);
        string? GetUserIdFromToken(string token);
        List<string> GetRolesFromToken(string token);
        List<string> GetPermissionsFromToken(string token);
        string? GetShopIdFromToken(string token);
        bool IsTokenValid(string token);
    }
}
