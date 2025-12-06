using Microsoft.AspNetCore.Mvc;
using PeShop.Dtos.Requests;
using PeShop.Interfaces;
using PeShop.Constants;
using System.Text;
using System.Text.Json;

namespace PeShop.MVC.Controllers
{
    [Route("manage")]
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJwtHelper _jwtHelper;
        private readonly IConfiguration _configuration;

        public LoginController(IHttpClientFactory httpClientFactory, IJwtHelper jwtHelper, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _jwtHelper = jwtHelper;
            _configuration = configuration;
        }

        [HttpGet("login")]
        public IActionResult Index()
        {
            // Nếu đã đăng nhập và có role Admin, redirect đến GHN Tool
            var token = HttpContext.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                var roles = _jwtHelper.GetRolesFromToken(token);
                if (roles.Contains(RoleConstants.Admin))
                {
                    return RedirectToAction("SwitchStatus", "GHNTool");
                }
            }

            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                ViewBag.Error = errors.Any() ? string.Join(", ", errors) : "Vui lòng nhập đầy đủ thông tin";
                return View("Index", request);
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var apiUrl = _configuration["ApiBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
                var loginUrl = $"{apiUrl}/Auth/login";

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(loginUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    
                    // API trả về JSON: {"error":null,"data":"token..."}
                    string token;
                    try
                    {
                        using var doc = JsonDocument.Parse(responseBody);
                        token = doc.RootElement.GetProperty("data").GetString() ?? "";
                    }
                    catch
                    {
                        // Fallback: nếu không phải JSON thì lấy trực tiếp
                        token = responseBody.Trim('"', ' ', '\n', '\r', '\t');
                    }

                    // Kiểm tra token hợp lệ
                    if (string.IsNullOrEmpty(token) || !_jwtHelper.IsTokenValid(token))
                    {
                        Console.WriteLine($"[DEBUG] Token validation failed");
                        ViewBag.Error = "Token không hợp lệ. Vui lòng thử lại.";
                        return View("Index", request);
                    }

                    // Kiểm tra role Admin trong token
                    var roles = _jwtHelper.GetRolesFromToken(token);
                    if (!roles.Contains(RoleConstants.Admin))
                    {
                        ViewBag.Error = "Bạn không có quyền truy cập. Chỉ Admin mới được phép đăng nhập.";
                        return View("Index", request);
                    }

                    // Lưu token vào session
                    HttpContext.Session.SetString("AccessToken", token);

                    // Redirect đến GHN Tool
                    return RedirectToAction("SwitchStatus", "GHNTool");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin đăng nhập.";
                    return View("Index", request);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi: {ex.Message}";
                return View("Index", request);
            }
        }

        [HttpGet("login/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}

