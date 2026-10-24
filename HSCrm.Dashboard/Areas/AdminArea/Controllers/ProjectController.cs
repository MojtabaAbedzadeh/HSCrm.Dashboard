using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
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
            try
            {
                var json = await _getListApi.GetApiList(apiUrl);
                var result = !string.IsNullOrWhiteSpace(json)
                    ? JsonConvert.DeserializeObject<ApiResponse<List<ProjectModel>>>(json)
                    : null;
                var model = result?.Data ?? new List<ProjectModel>();
                var tenantId = User.FindFirstValue("TenantId");
                ViewBag.Customers = await GetCustomerList(tenantId);
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطا در دریافت لیست پروژه‌ها: " + ex.Message;
                ViewBag.Customers = new List<CustomersDropDown>();
                return View(new List<ProjectModel>());
            }
        }

        [HttpGet]
        private async Task<List<CustomersDropDown>> GetCustomerList(string tenantId)
        {
            try
            {
                string apiUrlCustomer = "Customer/CustomerDropdownList?tenantId=" + tenantId;
                string json = await _getListApi.GetApiList(apiUrlCustomer);
                if (string.IsNullOrWhiteSpace(json)) return new List<CustomersDropDown>();
                var parsed = JObject.Parse(json);
                return JsonConvert.DeserializeObject<List<CustomersDropDown>>(parsed["data"]?.ToString() ?? "[]") ?? new List<CustomersDropDown>();
            }
            catch
            {
                return new List<CustomersDropDown>();
            }
        }
    }
}
