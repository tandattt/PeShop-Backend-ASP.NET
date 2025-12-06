using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PeShop.Interfaces;
using PeShop.Constants;

namespace PeShop.MVC.Controllers
{
    public abstract class BaseManageController : Controller
    {
        protected readonly IJwtHelper JwtHelper;

        protected BaseManageController(IJwtHelper jwtHelper)
        {
            JwtHelper = jwtHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Kiểm tra authentication
            var token = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(token))
            {
                context.Result = Redirect("/manage/login");
                return;
            }

            // Kiểm tra role Admin
            var roles = JwtHelper.GetRolesFromToken(token);
            if (!roles.Contains(RoleConstants.Admin))
            {
                context.Result = Redirect("/manage/login");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}

