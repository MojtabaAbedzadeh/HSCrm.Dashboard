using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
namespace HSCrm.Dashboard.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IConfiguration _config;

        public BaseController(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// این متد به عنوان یک فیلتر، قبل از اجرای هر اکشنی به صورت خودکار اجرا می‌شود
        /// و ViewBagها را برای تمام لایه‌ها و ویوها پر می‌کند.
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (User != null)
            {
                ViewBag.ApiAddress = _config["ApiAddress"];
                ViewBag.UserId = User.FindFirstValue("UserId");
                ViewBag.Token = User.FindFirstValue("Token");
                ViewBag.FirstName = User.FindFirstValue("FirstName");
                ViewBag.LastName = User.FindFirstValue("LastName");
                ViewBag.TenantId = User.FindFirstValue("TenantId");
                ViewBag.UserImageUrl = User.FindFirstValue("UserImageUrl");
                ViewBag.FiscalYearStatus = User.FindFirstValue("FiscalYearStatus");
                ViewBag.Email = User.FindFirstValue("Email");
                ViewBag.Mobile = User.FindFirstValue("MobilePhone");
            }
        }
    }
}