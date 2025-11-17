using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using System.Security.Claims;
namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }
    // [HttpPost("create-payment-url")]
    // [Authorize(Roles = RoleConstants.User)]
    // public async Task<IActionResult> CreatePaymentUrl([FromQuery] string orderId)
    // {
    //     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //     var result = await _paymentService.CreatePaymentUrlAsync(orderId, HttpContext, userId);
    //     return Ok(result);
    // }
    [HttpGet("vnpay-callback")]
    public async Task<IActionResult> VnpayCallback()
    {
        var result = await _paymentService.ProcessCallbackAsync(HttpContext);
        if (string.IsNullOrEmpty(result))
        {
            return BadRequest("Thanh toán không thành công");
        }
        // Kiểm tra nếu là mobile app
        var isMobile = HttpContext.Request.Headers["Platform"].ToString()?.ToLower() == "mobile"
            || HttpContext.Request.Headers["User-Agent"].ToString().Contains("Mobile")
            || HttpContext.Request.Headers["User-Agent"].ToString().Contains("App");

        if (isMobile)
        {
            // Extract orderId từ URL
            var orderId = ExtractOrderIdFromUrl(result);

            // Trả về JSON cho mobile app
            var response = new
            {
                Success = result.Contains("/success"), // Check nếu URL chứa "success"
                RedirectUrl = result,
                OrderId = orderId,
                DeepLink = $"peshop://payment/{(result.Contains("/success") ? "success" : "failed")}?orderId={orderId}",
                Message = result.Contains("/success") ? "Thanh toán thành công" : "Thanh toán thất bại"
            };
            Console.WriteLine("response: " + response.DeepLink);
            return Redirect(response.DeepLink);
        }
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