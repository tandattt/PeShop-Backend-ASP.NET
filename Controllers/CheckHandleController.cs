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
        if (!Request.Headers.TryGetValue("API-KEY", out var apiKey))
        {

            return Unauthorized("Missing Authorization header");
        }

        if (apiKey != _appSetting.ApiKeySystem)
        {
            return Forbid("Invalid API key");
        }
        return Ok(new { isRunning = HandleProduct.IsRunningHandleProduct });
    }
}