using HSCrm.Dashboard.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSCrm.Dashboard.Areas.UserArea.Controllers
{
    [Area("UserArea")]
    [Authorize(Roles = "Customer")]
    public class ProjectController : BaseController
    {
        public ProjectController(IConfiguration config) : base(config)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            ViewBag.ProjectId = id;
            return View();
        }
    }
}
