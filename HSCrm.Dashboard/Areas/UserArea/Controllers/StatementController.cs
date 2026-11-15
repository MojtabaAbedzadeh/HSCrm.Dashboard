using HSCrm.Dashboard.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSCrm.Dashboard.Areas.UserArea.Controllers
{
    [Area("UserArea")]
    [Authorize(Roles = "Customer")]
    public class StatementController : BaseController
    {
        public StatementController(IConfiguration config) : base(config)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
