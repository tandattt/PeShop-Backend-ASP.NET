using Microsoft.AspNetCore.Mvc;
using PeShop.Utilities;
using PeShop.Interfaces;
using PeShop.Dtos.GHN;
namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly IGHNUtil _ghnUtil;
    public TestController(IGHNUtil ghnUtil)
    {
        _ghnUtil = ghnUtil;
    }
    [HttpGet("get-list-province")]
    public async Task<ActionResult<ProvinceResponse>> GetListProvince()
    {
        var result = await _ghnUtil.GetListProvinceAsync();
        return Ok(result);
    }
    [HttpGet("get-list-district")]
    public async Task<ActionResult<DistrictResponse>> GetListDistrict(int provinceId)
    {
        var result = await _ghnUtil.GetListDistrictAsync(provinceId);
        return Ok(result);
    }
    [HttpGet("get-list-ward")]
    public async Task<ActionResult<WardResponse>> GetListWard(int districtId)
    {
        var result = await _ghnUtil.GetListWardAsync(districtId);
        return Ok(result);
    }
    [HttpPost("create-store")]
    public async Task<ActionResult<CreateStoreResponse>> CreateStore(CreateStoreRequest request)
    {
        var result = await _ghnUtil.CreateStoreAsync(request);
        return Ok(result);
    }

    [HttpPost("get-service")]
    public async Task<ActionResult<GetServiceResponse>> GetService(GetServiceRequest request)
    {
        var result = await _ghnUtil.GetServiceAsync(request);
        return Ok(result);
    }
    [HttpPost("calculate-fee-shipping")]
    public async Task<ActionResult<ShippingResponse>> CalculateFeeShipping(ShippingRequest request)
    {
        var result = await _ghnUtil.CalculateFeeShippingAsync(request);
        return Ok(result);
    }
    [HttpPost("create-order")]
    public async Task<ActionResult<GHNOrderResponse>> CreateOrder(GHNCreateOrderRequest request)
    {
        var result = await _ghnUtil.CreateOrderAsync(request);
        return Ok(result);
    }
}