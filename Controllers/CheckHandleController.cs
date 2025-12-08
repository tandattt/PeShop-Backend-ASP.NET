using Microsoft.AspNetCore.Mvc;
using PeShop.GlobalVariables;
using PeShop.Setting;

namespace PeShop.Controllers;

/// <summary>
/// Controller kiểm tra trạng thái xử lý sản phẩm - API-KEY
/// </summary>
/// <remarks>
/// <para><strong>🔑 Loại API:</strong> API-KEY - Yêu cầu API-KEY trong header</para>
/// <para><strong>📋 Mô tả:</strong> Kiểm tra trạng thái các background job xử lý sản phẩm.</para>
/// <para><strong>⚠️ Lưu ý:</strong> Chỉ dành cho hệ thống nội bộ.</para>
/// </remarks>
[ApiController]
[Route("[controller]")]
public class CheckHandleController : ControllerBase
{
    private readonly AppSetting _appSetting;

    public CheckHandleController(AppSetting appSetting)
    {
        _appSetting = appSetting;
    }

    /// <summary>
    /// Kiểm tra trạng thái xử lý sản phẩm - API-KEY
    /// </summary>
    /// <remarks>
    /// <para><strong>🔑 Xác thực:</strong> API-KEY trong header</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Kiểm tra xem background job xử lý sản phẩm có đang chạy không</li>
    ///   <li>Dùng để tránh chạy trùng job</li>
    ///   <li>Trả về trạng thái isRunning</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Headers:</strong></para>
    /// <ul>
    ///   <li><code>API-KEY: {your_api_key}</code></li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response:</strong></para>
    /// <ul>
    ///   <li><strong>200 OK:</strong> Trạng thái job</li>
    ///   <li><strong>401 Unauthorized:</strong> Thiếu API-KEY header</li>
    ///   <li><strong>403 Forbidden:</strong> API-KEY không hợp lệ</li>
    /// </ul>
    /// 
    /// <para><strong>📦 Response Data:</strong></para>
    /// <pre><code>{
    ///   "isRunning": true  // hoặc false
    /// }</code></pre>
    /// </remarks>
    /// <returns>Trạng thái xử lý sản phẩm</returns>
    [HttpGet("check-handle-product")]
    public IActionResult CheckHandleProduct()
    {
        if (!Request.Headers.TryGetValue("API-KEY", out var apiKey))
        {
            return Unauthorized("Missing Authorization header");
        }

        if (apiKey != _appSetting.ApiKeySystem)
        {
            return Forbid("Invalid API key");
        }
        return Ok(new { isRunning = HandleProduct.IsRunningHandleProduct });
    }
}