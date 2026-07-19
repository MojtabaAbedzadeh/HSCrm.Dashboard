using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    [Authorize(Roles = "Owner")]
    public class WarehouseController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;

        public WarehouseController(IConfiguration config, GetListApi getListApi) : base(config) 
        {
            _config = config;
            _getListApi = getListApi;
        }

        public async Task<IActionResult> Index()
        {
            string apiUrl = _config["ApiAddress"] + "Warehouse/GetWarehouses";
            string token = User.FindFirstValue("Token");

            var json = await _getListApi.GetApiList(apiUrl);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<WarehousesModel>>>(json);
            var model = result.Data;

            return View(model);
        }
    }
}
