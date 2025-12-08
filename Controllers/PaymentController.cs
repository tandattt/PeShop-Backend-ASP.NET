using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;

namespace PeShop.Controllers;

/// <summary>
/// Controller xử lý thanh toán - PUBLIC (Callback)
/// </summary>
/// <remarks>
/// <para><strong>📋 Mô tả:</strong> Xử lý callback từ cổng thanh toán VNPay.</para>
/// <para><strong>⚠️ Lưu ý:</strong> Endpoint callback được gọi bởi VNPay, không phải client.</para>
/// </remarks>
[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Callback xử lý kết quả thanh toán VNPay - PUBLIC (VNPay gọi)
    /// </summary>
    /// <remarks>
    /// <para><strong>🔓 Xác thực:</strong> Không yêu cầu (VNPay gọi trực tiếp)</para>
    /// <para><strong>📋 Mô tả:</strong></para>
    /// <ul>
    ///   <li>Endpoint được VNPay gọi sau khi user hoàn tất thanh toán</li>
    ///   <li>Xác thực chữ ký từ VNPay để đảm bảo tính hợp lệ</li>
    ///   <li>Cập nhật trạng thái đơn hàng dựa trên kết quả thanh toán</li>
    ///   <li>Redirect user về trang kết quả (web) hoặc deep link (mobile)</li>
    /// </ul>
    /// 
    /// <para><strong>📥 Query Parameters (từ VNPay):</strong></para>
    /// <ul>
    ///   <li><code>vnp_TxnRef</code>: Mã đơn hàng</li>
    ///   <li><code>vnp_ResponseCode</code>: Mã kết quả (00 = thành công)</li>
    ///   <li><code>vnp_SecureHash</code>: Chữ ký xác thực</li>
    ///   <li>... và các tham số khác từ VNPay</li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response (Web):</strong></para>
    /// <ul>
    ///   <li><strong>302 Redirect:</strong> Chuyển hướng đến trang kết quả</li>
    ///   <li>Success: <code>/payment/success?orderId=xxx</code></li>
    ///   <li>Failed: <code>/payment/failed?orderId=xxx</code></li>
    /// </ul>
    /// 
    /// <para><strong>📤 Response (Mobile):</strong></para>
    /// <ul>
    ///   <li><strong>302 Redirect:</strong> Deep link về app</li>
    ///   <li>Success: <code>peshop://payment/success?orderId=xxx</code></li>
    ///   <li>Failed: <code>peshop://payment/failed?orderId=xxx</code></li>
    /// </ul>
    /// 
    /// <para><strong>📱 Mobile Detection:</strong></para>
    /// <ul>
    ///   <li>Kiểm tra header <code>Platform: mobile</code></li>
    ///   <li>Nếu là mobile, redirect về deep link thay vì URL web</li>
    /// </ul>
    /// 
    /// <para><strong>⚠️ Lưu ý bảo mật:</strong></para>
    /// <ul>
    ///   <li>Luôn xác thực <code>vnp_SecureHash</code> trước khi xử lý</li>
    ///   <li>Kiểm tra <code>vnp_ResponseCode</code> để xác định kết quả</li>
    ///   <li>Không tin tưởng dữ liệu từ client, chỉ tin VNPay callback</li>
    /// </ul>
    /// </remarks>
    /// <returns>Redirect đến trang kết quả hoặc deep link</returns>
    [HttpGet("vnpay-callback")]
    public async Task<IActionResult> VnpayCallback()
    {
        var result = await _paymentService.ProcessCallbackAsync(HttpContext);
        if (string.IsNullOrEmpty(result))
        {
            return BadRequest("Thanh toán không thành công");
        }

        // Kiểm tra nếu là mobile app
        var platformHeader = HttpContext.Request.Headers["Platform"].ToString()?.ToLower();
        var isMobile = platformHeader == "mobile";

        if (isMobile)
        {
            var orderId = ExtractOrderIdFromUrl(result);
            var response = new
            {
                Success = result.Contains("/success"),
                RedirectUrl = result,
                OrderId = orderId,
                DeepLink = $"peshop://payment/{(result.Contains("/success") ? "success" : "failed")}?orderId={orderId}",
                Message = result.Contains("/success") ? "Thanh toán thành công" : "Thanh toán thất bại"
            };
            Console.WriteLine("[PaymentController] Mobile app detected - DeepLink: " + response.DeepLink);
            return Redirect(response.DeepLink);
        }

        // Web browser - redirect bình thường
        Console.WriteLine("[PaymentController] Web browser - Redirect to: " + result);
        return Redirect(result);
    }

    private string ExtractOrderIdFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
            return query.ContainsKey("orderId") ? query["orderId"].ToString() : "";
        }
        catch
        {
            return "";
        }
    }
}