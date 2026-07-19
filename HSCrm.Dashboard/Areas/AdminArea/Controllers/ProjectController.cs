using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    public class ProjectController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;

        public ProjectController(IConfiguration config, GetListApi getListApi) : base(config)
        {
            _getListApi = getListApi;
        }
        public async Task<IActionResult> Index()
        {
            string apiUrl = "Project/GetProjects";
            string token = User.FindFirstValue("Token");

            var json = await _getListApi.GetApiList(apiUrl);

            var result = JsonConvert.DeserializeObject<ApiResponse<List<ProjectModel>>>(json);
            var model = result?.Data ?? new List<ProjectModel>();

            var tenantId = User.FindFirstValue("TenantId");

            ViewBag.Customers = await GetCustomerList(tenantId);

            return View(model);
        }
        [HttpGet]
        private async Task<List<CustomersDropDown>> GetCustomerList(string tenantId)
        {
            string apiUrlCustomer = "Customer/CustomerDropdownList?tenantId=" + tenantId;
            string token = User.FindFirstValue("Token");
            
            string json = await _getListApi.GetApiList(apiUrlCustomer);

            var parsed = JObject.Parse(json);
            return JsonConvert.DeserializeObject<List<CustomersDropDown>>(parsed["data"]?.ToString() ?? "[]");
        }
    }
}
