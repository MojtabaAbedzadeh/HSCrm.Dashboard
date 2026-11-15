using HSCrm.Dashboard.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSCrm.Dashboard.Areas.UserArea.Controllers
{
    [Area("UserArea")]
    [Authorize(Roles = "Customer")]
    public class ProfileController : BaseController
    {
        public ProfileController(IConfiguration config) : base(config)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
