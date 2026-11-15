using HSCrm.Dashboard.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    public class EmployeeController : BaseController
    {
        public EmployeeController(IConfiguration config) : base(config)
        {
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Customer") && !User.IsInRole("Owner") && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Redirect("/UserArea/Home/Index");
            }

            return View();
        }
    }
}
