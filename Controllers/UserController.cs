using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeShop.Constants;
using PeShop.Dtos.Requests;
using PeShop.Services.Interfaces;
using System.Security.Claims;

namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Lấy thông tin user hiện tại
    /// </summary>
    /// <returns>Thông tin user</returns>
    [HttpGet("me")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Token không hợp lệ");
        }

        try
        {
            var userInfo = await _userService.GetUserInfoAsync(userId);
            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("me")]
    [Authorize(Roles = RoleConstants.User)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Token không hợp lệ");
        }

        try
        {
            var result = await _userService.UpdateUserInfoAsync(userId, request);
            if (result)
            {
                return Ok("Cập nhật thông tin thành công");
            }
            return BadRequest("Cập nhật thông tin thất bại");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

