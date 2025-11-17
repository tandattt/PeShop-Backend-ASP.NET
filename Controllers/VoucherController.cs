using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using PeShop.Dtos.Requests;
using System.Security.Claims;
using PeShop.Dtos.Responses;
namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class VoucherController : ControllerBase
{
    private readonly IVoucherService _voucherService;
    public VoucherController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    [HttpGet("get-vouchers")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetVouchers()
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _voucherService.GetVouchersAsync(userId);
        return Ok(result);
    }

    [HttpGet("check-eligibility")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<CheckVoucherEligibilityResponse>> CheckVoucherEligibility([FromQuery] string orderId)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User not found");
        }
        if (string.IsNullOrEmpty(orderId))
        {
            return BadRequest("Order not found");
        }
        
        var result = await _voucherService.CheckVoucherEligibilityAsync(userId, orderId);
        return Ok(result);
    }

    [HttpPost("apply-voucher-system")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> ApplyVoucherSystem([FromBody] ApplyVoucherSystemRequest request)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User not found");
        }
        return Ok(await _voucherService.ApplyVoucherSystemAsync(userId, request));
    }

    [HttpPost("apply-voucher-shop")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> ApplyVoucherShop([FromBody] ApplyVoucherShopRequest request)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User not found");
        }
        return Ok(await _voucherService.ApplyVoucherShopAsync(userId, request));
    }
}