using HSCrm.Dashboard.Controllers;
using HSCrm.Dashboard.Costants;
using HSCrm.Dashboard.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    public class HomeController : BaseController
    {
        public HomeController(IConfiguration config) : base(config)
        {
        }

        public IActionResult Index()
        {
            var isFullAccess = User.IsInRole("Owner")
                || User.IsInRole("Admin")
                || User.IsInRole("SuperAdmin")
                || User.HasPermission(Permissions.AccountingReports.View);

            if (!isFullAccess)
            {
                return Redirect("/AdminArea/Employee/Index");
            }

            ViewBag.FiscalYearStatus = User.FindFirstValue("FiscalYearStatus");
            return View();
        }
    }
}
