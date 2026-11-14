using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using HSCrm.Dashboard.Costants;
using Microsoft.AspNetCore.Authorization;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    [Authorize(Policy = "Permission:" + Permissions.Inventories.View)]
    public class InventoryController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;

        public InventoryController(IConfiguration config, GetListApi getListApi) : base(config)
        {
            _getListApi = getListApi;
        }

        public IActionResult KardexReport()
        {
            return View();
        }
    }
}
