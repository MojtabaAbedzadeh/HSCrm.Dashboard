using HSCrm.Dashboard.Controllers;
using Microsoft.AspNetCore.Mvc;
using HSCrm.Dashboard.Costants;
using Microsoft.AspNetCore.Authorization;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    [Authorize(Policy = "Permission:" + Permissions.Tenants.View)]
    public class SettingsController : BaseController
    {
        public SettingsController(IConfiguration config) : base(config)
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
