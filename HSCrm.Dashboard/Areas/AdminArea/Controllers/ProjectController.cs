using HSCrm.BussinessLogic.PublicMethod;
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
    [Authorize(Roles = "Admin")]
    public class ProjectController : Controller
    {
        private readonly IConfiguration _config;

        public ProjectController(IConfiguration config)
        {
            _config = config;
        }
        public async Task<IActionResult> Index()
        {
            string apiUrl = _config["ApiAddress"] + "Project/GetProjects";
            string token = User.FindFirstValue("Token");
            GetListApi GA = new GetListApi();
            var json = await GA.GetApiList(apiUrl, token);

            var result = JsonConvert.DeserializeObject<ApiResponse<List<ProjectModel>>>(json);
            var model = result?.Data ?? new List<ProjectModel>();

            var tenantId = User.FindFirstValue("TenantId");

            ViewBag.Customers = await GetCustomerList(tenantId);

            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.Token = User.FindFirstValue("Token");
            ViewBag.FirstName = User.FindFirstValue("FirstName");
            ViewBag.LastName = User.FindFirstValue("LastName");
            ViewBag.UserImageUrl = User.FindFirstValue("UserImageUrl");
            ViewBag.FiscalYearStatus = User.FindFirstValue("FiscalYearStatus");

            return View(model);
        }


        [HttpGet]
        private async Task<List<CustomersDropDown>> GetCustomerList(string tenantId)
        {
            string apiUrlCustomer = _config["ApiAddress"] + "Customer/CustomerDropdownList?tenantId=" + tenantId;
            string token = User.FindFirstValue("Token");
            GetListApi getListCustomer = new GetListApi();
            string json = await getListCustomer.GetApiList(apiUrlCustomer, token);

            var parsed = JObject.Parse(json);
            return JsonConvert.DeserializeObject<List<CustomersDropDown>>(parsed["data"]?.ToString() ?? "[]");
        }
    }
}
