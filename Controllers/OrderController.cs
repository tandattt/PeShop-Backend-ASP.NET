using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using System.Security.Claims;
using PeShop.Dtos.Requests;
namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    [HttpPost("create-virtual-order")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> CreateVirtualOrder([FromBody] OrderVirtualRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok(await _orderService.CreateVirtualOrder(request,userId));
    }
}