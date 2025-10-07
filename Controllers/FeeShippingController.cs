using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
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
        var result = await _feeShippingService.FeeShippingAsync(request);
        return Ok(result);
    }
}