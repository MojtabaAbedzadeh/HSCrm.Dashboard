using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    public class SupplierController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;
        public SupplierController(IConfiguration config, GetListApi getListApi) : base(config) 
        {
            _getListApi = getListApi;
        }
        public async Task<IActionResult> Index()
        {
            string apiUrl = "Supplier/GetSupplier";

            var json = await _getListApi.GetApiList(apiUrl);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<SupplierModel>>>(json);
            var model = result.Data;

            return View(model);
        }
    }
}
