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
    public class ExpenseController : Controller
    {
        private readonly IConfiguration _config;
        public ExpenseController(IConfiguration config)
        {
            _config = config;
        }
        public async Task<IActionResult> Index()
        {
            string apiUrl = _config["ApiAddress"] + "Expense/GetExpense";
            string token = User.FindFirstValue("Token");
            GetListApi GA = new GetListApi();
            var json = await GA.GetApiList(apiUrl, token);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<ExpenseModel>>>(json);
            var model = result.Data;

            var tenantId = User.FindFirstValue("TenantId");
            ViewBag.Projects = await GetProjectList(tenantId);

            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.TenantId = User.FindFirstValue("TenantId");
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.Token = User.FindFirstValue("Token");
            ViewBag.FirstName = User.FindFirstValue("FirstName");
            ViewBag.LastName = User.FindFirstValue("LastName");
            ViewBag.UserImageUrl = User.FindFirstValue("UserImageUrl");
            ViewBag.FiscalYearStatus = User.FindFirstValue("FiscalYearStatus");

            return View(model);
        }
        [HttpGet]
        private async Task<List<ProjetcsDropDown>> GetProjectList(string tenantId)
        {
            string apiUrlProject = _config["ApiAddress"] + "Project/ProjectDropdownList?tenantId=" + tenantId;
            string token = User.FindFirstValue("Token");
            GetListApi getListProject = new GetListApi();
            string json = await getListProject.GetApiList(apiUrlProject, token);

            var parsed = JObject.Parse(json);
            return JsonConvert.DeserializeObject<List<ProjetcsDropDown>>(
                parsed["data"]?.ToString() ?? "[]"
            );
        }
        [HttpGet]
        public async Task<IActionResult> ProjectExpenses(int projectId)
        {
            string apiUrl = _config["ApiAddress"] + "Expense/GetByProjectId?projectId=" + projectId;
            string token = User.FindFirstValue("Token");
            GetListApi GA = new GetListApi();
            var json = await GA.GetApiList(apiUrl, token);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<ExpenseModel>>>(json);
            var model = result.Data;

            var tenantId = User.FindFirstValue("TenantId");

            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.TenantId = User.FindFirstValue("TenantId");
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.Token = User.FindFirstValue("Token");
            ViewBag.FirstName = User.FindFirstValue("FirstName");
            ViewBag.LastName = User.FindFirstValue("LastName");
            ViewBag.UserImageUrl = User.FindFirstValue("UserImageUrl");
            ViewBag.FiscalYearStatus = User.FindFirstValue("FiscalYearStatus");

            return View(model);
        }
    }
}
