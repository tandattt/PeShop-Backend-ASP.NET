using Microsoft.AspNetCore.Mvc;
using PeShop.Interfaces;

namespace PeShop.MVC.Controllers
{
    [Route("manage")]
    public class ManageController : BaseManageController
    {
        public ManageController(IJwtHelper jwtHelper) : base(jwtHelper)
        {
        }

        [HttpGet]
        [HttpGet("index")]
        public IActionResult Index()
        {
            // Redirect to GHN Tool
            return RedirectToAction("SwitchStatus", "GHNTool");
        }
    }
}

