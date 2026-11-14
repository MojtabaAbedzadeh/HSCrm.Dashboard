using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using Microsoft.AspNetCore.Mvc;
using HSCrm.Dashboard.Costants;
using Microsoft.AspNetCore.Authorization;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    [Authorize(Policy = "Permission:" + Permissions.FiscalYears.View)]
    public class FiscalYearController : BaseController
    {
        private readonly GetListApi _getListApi;

        public FiscalYearController(IConfiguration config, GetListApi getListApi) : base(config)
        {
            _getListApi = getListApi;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
