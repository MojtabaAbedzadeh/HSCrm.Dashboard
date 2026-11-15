using HSCrm.Dashboard.Controllers;
using HSCrm.Dashboard.Costants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    [Authorize(Policy = "Permission:" + Permissions.AuditTrail.View)]
    public class AuditTrailController : BaseController
    {
        public AuditTrailController(IConfiguration config) : base(config)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
