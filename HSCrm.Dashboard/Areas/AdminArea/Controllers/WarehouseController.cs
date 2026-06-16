using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    [Authorize(Roles = "Admin")]
    public class WarehouseController : Controller
    {
        private readonly IConfiguration _config;

        public WarehouseController(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            string apiUrl = _config["ApiAddress"] + "Warehouse/GetWarehouses";
            string token = User.FindFirstValue("Token");
            GetListApi GA = new GetListApi();
            var json = await GA.GetApiList(apiUrl, token);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<WarehousesModel>>>(json);
            var model = result.Data;

            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.TenantId = User.FindFirstValue("TenantId");
            ViewBag.Token = User.FindFirstValue("Token");
            ViewBag.FirstName = User.FindFirstValue("FirstName");
            ViewBag.LastName = User.FindFirstValue("LastName");
            ViewBag.UserImageUrl = User.FindFirstValue("UserImageUrl");
            ViewBag.FiscalYearStatus = User.FindFirstValue("FiscalYearStatus");

            return View(model);
        }
    }
}
