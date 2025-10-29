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
        return Redirect(result);
    }
}