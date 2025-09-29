using Microsoft.AspNetCore.Mvc;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Services;

namespace PeShop.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="request">Thông tin đăng nhập</param>
        /// <returns>Token và thông tin user</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);

            Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
            {
                HttpOnly = true,         // Không cho JS đọc
                Secure = true,           // Chỉ truyền qua HTTPS
                SameSite = SameSiteMode.Strict, // Hạn chế CSRF
                Expires = DateTimeOffset.UtcNow.AddHours(48) // hạn token = 48h
            });
            return Ok(response.AccessToken);

        }

        // [HttpPost("refresh")]
        // public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        // {
        //     return Ok(new { message = "Refresh token not implemented" });
        // }



        /// <summary>
        /// Đăng ký user mới
        /// </summary>
        /// <param name="request">Thông tin đăng ký</param>
        /// <returns>Thông tin user đã tạo</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);

        }


    }
}
