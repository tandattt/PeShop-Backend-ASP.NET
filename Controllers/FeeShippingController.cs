using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using PeShop.Dtos.Responses;
using System.Security.Claims;
namespace Controllers;

[ApiController]
[Route("[controller]")]
public class FeeShippingController : ControllerBase
{
    private readonly IFeeShippingService _feeShippingService;
    public FeeShippingController(IFeeShippingService feeShippingService)
    {
        _feeShippingService = feeShippingService;
    }

    [HttpPost("get-fee-shipping")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetFeeShipping([FromBody] ListFeeShippingRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _feeShippingService.FeeShippingAsync(request, userId);
        return Ok(result);
    }

    [HttpPost("apply-list-fee-shipping")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> ApplyListFeeShipping([FromBody] ApplyListFeeShippingRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _feeShippingService.ApplyFeeShippingAsync(request, userId);
        return Ok(result);
    }
}