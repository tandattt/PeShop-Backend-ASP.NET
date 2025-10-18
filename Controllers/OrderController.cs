using Microsoft.AspNetCore.Mvc;
using PeShop.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PeShop.Constants;
using System.Security.Claims;
namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
//     private readonly IOrderService _orderService;
//     public OrderController(IOrderService orderService)
//     {
//         _orderService = orderService;
//     }
//     [HttpGet("calculate-order-total")]
//     [Authorize(Roles = RoleConstants.User)]
//     public async Task<IActionResult> CalculateOrderTotal([FromBody] CalculateOrderTotalRequest request)
//     {
//         return Ok(await _orderService.CalculateOrderTotal(request));
//     }
}