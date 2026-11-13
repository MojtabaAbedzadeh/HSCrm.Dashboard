using HSCrm.Dashboard.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
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
