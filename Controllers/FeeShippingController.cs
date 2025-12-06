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
    public async Task<ActionResult<List<FeeShippingResponse>>> GetFeeShipping([FromBody] ListFeeShippingRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _feeShippingService.FeeShippingAsync(request, userId);
        return Ok(result);
    }

    [HttpPost("apply-list-fee-shipping")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> ApplyListFeeShipping([FromBody] ApplyListFeeShippingRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _feeShippingService.ApplyFeeShippingAsync(request, userId);
        return Ok(result);
    }
    
    // V2 - GHN only endpoints
    [HttpPost("get-fee-shipping-v2")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<ListFeeShippingV2Response>> GetFeeShippingV2([FromBody] FeeShippingV2Request request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _feeShippingService.FeeShippingV2Async(request, userId);
        return Ok(result);
    }
    
    [HttpPost("apply-fee-shipping-v2")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> ApplyFeeShippingV2([FromBody] ApplyFeeShippingV2Request request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _feeShippingService.ApplyFeeShippingV2Async(request, userId);
        return Ok(result);
    }
}