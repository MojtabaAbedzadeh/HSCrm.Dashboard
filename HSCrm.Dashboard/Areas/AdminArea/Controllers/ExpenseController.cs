using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Dashboard.Costants;
using HSCrm.Dashboard.Extensions;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
            if (!User.HasPermission(Permissions.Expenses.View) && !User.HasPermission(Permissions.Expenses.Create))
            {
                return Forbid();
            }

            var tenantId = User.FindFirstValue("TenantId");
            ViewBag.Projects = await GetProjectList(tenantId);

            string apiUrl = "Expense/GetExpense";
            try
            {
                var json = await _getListApi.GetApiList(apiUrl);
                var result = !string.IsNullOrWhiteSpace(json)
                    ? JsonConvert.DeserializeObject<ApiResponse<List<ExpenseModel>>>(json)
                    : null;
                var model = result?.Data ?? new List<ExpenseModel>();
                return View(model);
            }
            catch (AccessDeniedException)
            {
                if (!User.HasPermission(Permissions.Expenses.Create))
                {
                    ViewBag.ErrorMessage = "دسترسی به مشاهده هزینه‌ها مجاز نمی‌باشد.";
                }
                return View(new List<ExpenseModel>());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطا در دریافت هزینه‌ها: " + ex.Message;
                return View(new List<ExpenseModel>());
            }
        }

        [HttpGet]
        private async Task<List<ProjetcsDropDown>> GetProjectList(string tenantId)
        {
            try
            {
                string apiUrl = "Project/ProjectDropdownList?tenantId=" + tenantId;
                var json = await _getListApi.GetApiList(apiUrl);
                if (string.IsNullOrWhiteSpace(json)) return new List<ProjetcsDropDown>();
                var result = JObject.Parse(json);
                return JsonConvert.DeserializeObject<List<ProjetcsDropDown>>(
                    result["data"]?.ToString() ?? "[]"
                ) ?? new List<ProjetcsDropDown>();
            }
            catch
            {
                return new List<ProjetcsDropDown>();
            }
        }

        [HttpGet]
        [Authorize(Policy = "Permission:" + Permissions.Expenses.View)]
        public async Task<IActionResult> ProjectExpenses(int projectId)
        {
            string apiUrl = "Expense/GetByProjectId?projectId=" + projectId;
            try
            {
                var json = await _getListApi.GetApiList(apiUrl);
                var result = !string.IsNullOrWhiteSpace(json)
                    ? JsonConvert.DeserializeObject<ApiResponse<List<ExpenseModel>>>(json)
                    : null;
                var model = result?.Data ?? new List<ExpenseModel>();
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطا در دریافت هزینه‌های پروژه: " + ex.Message;
                return View(new List<ExpenseModel>());
            }
        }
    }
}
