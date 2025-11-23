using Microsoft.AspNetCore.Mvc;
using PeShop.GlobalVariables;
using PeShop.Setting;
namespace PeShop.Controllers;

[ApiController]
[Route("[controller]")]
public class CheckHandleController : ControllerBase
{
    private readonly AppSetting _appSetting;
    public CheckHandleController(AppSetting appSetting)
    {
        _appSetting = appSetting;
    }
    [HttpGet("check-handle-product")]
    public IActionResult CheckHandleProduct()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
        {

            return Unauthorized("Missing Authorization header");
        }

        var token = authHeader.ToString().Replace("Bearer ", "");
        if (token != _appSetting.ApiKeySystem)
        {
            return Forbid("Invalid API key");
        }
        return Ok(new { isRunning = HandleProduct.IsRunningHandleProduct });
    }
}