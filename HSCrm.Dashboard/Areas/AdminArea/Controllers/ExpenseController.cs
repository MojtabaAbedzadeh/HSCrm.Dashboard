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
    public class ExpenseController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;
        public ExpenseController(IConfiguration config, GetListApi getListApi) : base(config)
        {
            _getListApi = getListApi;
        }
        public async Task<IActionResult> Index()
        {
            string apiUrl = "Expense/GetExpense"; // اگر BaseAddress را در Program.cs ست کردی، فقط ادامه آدرس کافیست

            // اضافه کردن await در اینجا الزامی است
            var json = await _getListApi.GetApiList(apiUrl);

            // حالا json یک رشته واقعی است و Deserialize به درستی کار می‌کند
            var result = JsonConvert.DeserializeObject<ApiResponse<List<ExpenseModel>>>(json);
            var model = result?.Data;

            var tenantId = User.FindFirstValue("TenantId");
            ViewBag.Projects = await GetProjectList(tenantId);

            return View(model);
        }
        [HttpGet]
        private async Task<List<ProjetcsDropDown>> GetProjectList(string tenantId)
        {
            string apiUrl = "Project/ProjectDropdownList?tenantId=" + tenantId;

            var json = await _getListApi.GetApiList(apiUrl);
            var result = JObject.Parse(json);

            return JsonConvert.DeserializeObject<List<ProjetcsDropDown>>(
                result["data"]?.ToString() ?? "[]"
            );
        }
        [HttpGet]
        public async Task<IActionResult> ProjectExpenses(int projectId)
        {
            string apiUrl = "Expense/GetByProjectId?projectId=" + projectId;

            var json = await _getListApi.GetApiList(apiUrl);

            var result = JsonConvert.DeserializeObject<ApiResponse<List<ExpenseModel>>>(json);
            var model = result?.Data;

            var tenantId = User.FindFirstValue("TenantId");

            return View(model);
        }
    }
}