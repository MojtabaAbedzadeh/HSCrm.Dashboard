using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Dashboard.Costants;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    [Authorize(Policy = "Permission:" + Permissions.ProjectProgress.View)]
    public class ProjectProgressController : BaseController
    {
        private readonly GetListApi _getListApi;

        public ProjectProgressController(IConfiguration config, GetListApi getListApi) : base(config)
        {
            _getListApi = getListApi;
        }

        public async Task<IActionResult> Index(int? projectId = null)
        {
            var tenantId = User.FindFirstValue("TenantId");
            ViewBag.Projects = await GetProjectListAsync(tenantId);
            ViewBag.SelectedProjectId = projectId;
            return View();
        }

        private async Task<List<ProjetcsDropDown>> GetProjectListAsync(string? tenantId)
        {
            try
            {
                string apiUrl = "Project/ProjectDropdownList?tenantId=" + tenantId;
                var json = await _getListApi.GetApiList(apiUrl);
                if (string.IsNullOrWhiteSpace(json)) return new List<ProjetcsDropDown>();
                var parsed = JObject.Parse(json);
                return JsonConvert.DeserializeObject<List<ProjetcsDropDown>>(parsed["data"]?.ToString() ?? "[]")
                    ?? new List<ProjetcsDropDown>();
            }
            catch
            {
                return new List<ProjetcsDropDown>();
            }
        }
    }
}
