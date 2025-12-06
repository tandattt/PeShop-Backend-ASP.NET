using Microsoft.AspNetCore.Mvc;
using PeShop.Interfaces;
using PeShop.Dtos.GHN;
using PeShop.Dtos.Requests;
using Microsoft.AspNetCore.Authorization;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Helpers;

namespace PeShop.Controllers;

[ApiController]
[Route("ghn")]
[Authorize]
public class GHNController : ControllerBase
{
    private readonly IGHNUtil _ghnUtil;
    private readonly IOrderRepository _orderRepository;

    public GHNController(IGHNUtil ghnUtil, IOrderRepository orderRepository)
    {
        _ghnUtil = ghnUtil;
        _orderRepository = orderRepository;
    }

    [HttpGet("get-list-province")]
    [AllowAnonymous]
    public async Task<ActionResult<ProvinceResponse>> GetListProvince()
    {
        var result = await _ghnUtil.GetListProvinceAsync();
        return Ok(result);
    }

    [HttpGet("get-list-district")]
    [AllowAnonymous]
    public async Task<ActionResult<DistrictResponse>> GetListDistrict(int provinceId)
    {
        var result = await _ghnUtil.GetListDistrictAsync(provinceId);
        return Ok(result);
    }

    [HttpGet("get-list-ward")]
    [AllowAnonymous]
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

    [HttpPost("cancel-order")]
    public async Task<ActionResult<CancelOrderResponse>> CancelOrder(string orderCode)
    {
        var result = await _ghnUtil.CancelOrderAsync(orderCode);
        return Ok(result);
    }

    [HttpPost("switch-order-status")]
    [AllowAnonymous]
    public async Task<ActionResult<SwitchOrderStatusResponse>> SwitchOrderStatus([FromBody] SwitchOrderStatusRequest request)
    {
        try
        {
            var result = await _ghnUtil.SwitchOrderStatusAsync(request);

            
            if (result.code != 200)
            {
                return BadRequest(new { message = "Lỗi khi chuyển trạng thái đơn hàng", error = result.message });
            }
            return Ok(result);
        }
        catch (Exception)
        {
            return BadRequest(new { message = "Lỗi khi chuyển trạng thái đơn hàng", error = "Lỗi hệ thống" });
        }
    }
}
