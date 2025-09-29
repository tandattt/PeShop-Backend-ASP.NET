using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Interfaces;
using PeShop.Constants;
namespace PeShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IJwtHelper _jwtUtil;

        public TestController(IJwtHelper jwtUtil)
        {
            _jwtUtil = jwtUtil;
        }

        /// <summary>
        /// Test API không cần authentication
        /// </summary>
        [HttpGet("public")]
        [Authorize(Roles = RoleConstants.User)]
        public IActionResult PublicTest()
        {
            return Ok(new { message = "Đây là API public, không cần authentication" });
        }

        /// <summary>
        /// Test API cần authentication
        /// </summary>
        [HttpGet("protected")]
        [Authorize]
        public IActionResult ProtectedTest()
        {
            var userId = User.FindFirst("sub")?.Value;
            var roles = User.FindAll("authorities").Select(c => c.Value).ToList();
            
            return Ok(new 
            { 
                message = "Đây là API protected, cần authentication",
                userId = userId,
                roles = roles
            });
        }

        /// <summary>
        /// Test tạo token thủ công
        /// </summary>
        [HttpPost("create-token")]
        [Authorize(Roles = "User")]
        public IActionResult CreateToken([FromBody] string userId)
        {
            var payload = new Dtos.Common.JwtPayloadDto
            {
                Sub = userId,
                Authorities = new List<string> { "User" }
            };

            var token = _jwtUtil.GenerateToken(payload);
            // var refreshToken = _jwtUtil.GenerateRefreshToken();

            return Ok(new
            {
                AccessToken = token,
                // RefreshToken = refreshToken,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds()
            });
        }
    }
}
