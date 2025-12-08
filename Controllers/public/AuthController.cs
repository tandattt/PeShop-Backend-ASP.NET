using Microsoft.AspNetCore.Mvc;
using PeShop.Dtos.Requests;
using PeShop.Services;

namespace PeShop.Controllers
{
    /// <summary>
    /// Controller xử lý xác thực người dùng - PUBLIC API
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Loại API:</strong> Public - Không yêu cầu xác thực</para>
    /// <para><strong>📋 Mô tả:</strong> Cung cấp các endpoint đăng nhập và đăng ký tài khoản cho người dùng.</para>
    /// </remarks>
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
        /// Đăng nhập hệ thống - PUBLIC
        /// </summary>
        /// <remarks>
        /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Xác thực thông tin đăng nhập của người dùng</li>
        ///   <li>Trả về Access Token để sử dụng cho các API khác</li>
        ///   <li>Refresh Token được lưu trong HttpOnly Cookie (bảo mật cao)</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Request Body:</strong></para>
        /// <pre><code>{
        ///   "email": "user@example.com",
        ///   "password": "your_password"
        /// }</code></pre>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Trả về Access Token (string)</li>
        ///   <li><strong>400 Bad Request:</strong> Thông tin đăng nhập không hợp lệ</li>
        /// </ul>
        /// 
        /// <para><strong>🍪 Cookie:</strong></para>
        /// <ul>
        ///   <li><code>refreshToken</code> - HttpOnly, Secure, SameSite=Strict, Expires=48h</li>
        /// </ul>
        /// </remarks>
        /// <param name="request">Thông tin đăng nhập (email, password)</param>
        /// <returns>Access Token để xác thực các request tiếp theo</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);

            if (response != null)
            {
                Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(48)
                });
                return Ok(response.AccessToken);
            }
            return BadRequest("Login failed");
        }

        /// <summary>
        /// Đăng ký tài khoản mới - PUBLIC
        /// </summary>
        /// <remarks>
        /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu</para>
        /// <para><strong>📋 Mô tả:</strong></para>
        /// <ul>
        ///   <li>Tạo tài khoản người dùng mới trong hệ thống</li>
        ///   <li>Email phải là duy nhất và chưa được đăng ký</li>
        ///   <li>Yêu cầu xác thực OTP qua email trước khi đăng ký</li>
        /// </ul>
        /// 
        /// <para><strong>📥 Request Body:</strong></para>
        /// <pre><code>{
        ///   "email": "user@example.com",
        ///   "password": "your_password",
        ///   "name": "Tên người dùng",
        ///   "phone": "0123456789"
        /// }</code></pre>
        /// 
        /// <para><strong>📤 Response:</strong></para>
        /// <ul>
        ///   <li><strong>200 OK:</strong> Thông tin user đã tạo thành công</li>
        ///   <li><strong>400 Bad Request:</strong> Email đã tồn tại hoặc dữ liệu không hợp lệ</li>
        /// </ul>
        /// </remarks>
        /// <param name="request">Thông tin đăng ký tài khoản</param>
        /// <returns>Thông tin user đã được tạo</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
    }
}
