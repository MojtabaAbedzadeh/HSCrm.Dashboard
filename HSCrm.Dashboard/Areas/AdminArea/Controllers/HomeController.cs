using HSCrm.Dashboard.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    public class HomeController : BaseController
    {
        private readonly IConfiguration _config;
        public HomeController(IConfiguration config) : base(config)
        {
        }
        public IActionResult Index()
        {
            ViewBag.FiscalYearStatus = User.FindFirstValue("FiscalYearStatus");
            return View();
        }
    }
}
