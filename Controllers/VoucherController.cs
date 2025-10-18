using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using PeShop.Dtos.Requests;
using System.Security.Claims;
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

    [HttpPost("check-eligibility")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> CheckVoucherEligibility([FromBody] CheckVoucherEligibilityRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.Items == null || !request.Items.Any())
        {
            return BadRequest("Danh sách sản phẩm không được rỗng");
        }

        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        
        var result = await _voucherService.CheckVoucherEligibilityAsync(request, userId);
        return Ok(result);
    }
}