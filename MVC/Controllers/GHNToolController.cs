using Microsoft.AspNetCore.Mvc;
using PeShop.Dtos.Requests;
using PeShop.Interfaces;

namespace PeShop.MVC.Controllers;

[Route("manage")]
public class GHNToolController : BaseManageController
{
    public GHNToolController(IJwtHelper jwtHelper) : base(jwtHelper)
    {
    }

    [HttpGet("ghn-tool/switch-status")]
    public IActionResult SwitchStatus()
    {
        return View();
    }
}

