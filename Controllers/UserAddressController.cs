using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Constants;
using PeShop.Dtos.Requests;
using PeShop.Services.Interfaces;
using System.Security.Claims;
namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class UserAddressController : ControllerBase
{
    private readonly IUserAddressService _userAddressService;
    public UserAddressController(IUserAddressService userAddressService)
    {
        _userAddressService = userAddressService;
    }

    [HttpPost("create-list-address")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> Create([FromBody] UserAddressRequest request)
    {
        string user_id =User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _userAddressService.CreateUserAddressAsync(request, user_id);
        return Ok(result);
    }

    [HttpPut("update-address")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> Update([FromQuery] string id, [FromBody] UserAddressRequest request)
    {
        string user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _userAddressService.UpdateUserAddressAsync(id, request, user_id);
        return Ok(result);
    }

    [HttpDelete("delete-address")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> Delete([FromQuery] string id)
    {
        string user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _userAddressService.DeleteUserAddressAsync(id, user_id);
        return Ok(result);
    }

    [HttpGet("get-list-address")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetListAddress()
    {
        string user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _userAddressService.GetListAddressAsync(user_id);
        return Ok(result);
    }

    [HttpGet("get-address-default")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetAddressDefault()
    {
        string user_id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var result = await _userAddressService.GetAddressDefaultAsync(user_id);
        return Ok(result);
    }
}
