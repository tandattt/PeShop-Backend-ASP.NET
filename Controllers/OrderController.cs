using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using System.Security.Claims;
using PeShop.Dtos.Requests;
using PeShop.Models.Enums;
using PeShop.Dtos.Responses;
namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    public OrderController(IOrderService orderService, IPaymentService paymentService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
    }
    [HttpPost("create-virtual-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<CreateVirtualOrderResponse>> CreateVirtualOrder([FromBody] OrderVirtualRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(await _orderService.CreateVirtualOrder(request,userId));
    }
    [HttpPut("update-virtual-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<CreateVirtualOrderResponse>> UpdateVirtualOrder([FromBody] UpdateVirtualOrderRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(await _orderService.UpdateVirtualOrder(request, userId));
    }

    [HttpDelete("delete-virtual-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> DeleteVirtualOrder([FromQuery] string orderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(await _orderService.DeleteVirtualOrder(orderId, userId));
    }

    [HttpGet("Calclulate-order-total")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<CreateVirtualOrderResponse>> CalclulateOrderTotal([FromQuery] string orderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(await _orderService.CalclulateOrderTotal(orderId,userId));
    }
    [HttpPost("create-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<StatusResponse>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(request.PaymentMethod == PaymentMethod.COD){
            return Ok(await _orderService.CreateOrderCODAsync(request.OrderId,userId));
        }
        else if(request.PaymentMethod == PaymentMethod.VNPay){
            return Ok(await _paymentService.CreatePaymentUrlAsync(request.OrderId, HttpContext, userId));
        }
        else{
            return BadRequest("Phương thức thanh toán không hợp lệ");
        }
    }

    [HttpGet("get-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<ActionResult<List<OrderResponse>>> GetOrder()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var order = await _orderService.GetOrderAsync( userId);
        if (order == null)
        {
            return NotFound("Đơn hàng không tồn tại");
        }
        return Ok(order);
    }

    [HttpGet("get-order-detail")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetOrderDetail([FromQuery] string orderId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var order = await _orderService.GetOrderDetailAsync(orderId, userId);
        if (order == null)
        {
            return NotFound("Đơn hàng không tồn tại");
        }
        return Ok(order);
    }
}