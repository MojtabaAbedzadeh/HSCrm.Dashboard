using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    [Authorize(Roles = "Owner")]
    public class RoleController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;

        public RoleController(IConfiguration config, GetListApi getListApi) : base(config)
        {
            _config = config;
            _getListApi = getListApi;
        }
        private string ApiUrl(string endpoint)
        {
            return _config["ApiAddress"] + endpoint;
        }
        private string Token()
        {
            return User.FindFirstValue("Token");
        }
        public async Task<IActionResult> Index()
        {
            var json = await _getListApi.GetApiList(ApiUrl("Role/GetRoles"));

            var result = JsonConvert.DeserializeObject<ApiResponse<List<RoleModel>>>(json);
            var model = result?.Data ?? new List<RoleModel>();

            return View(model);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new RoleCreateDto());
        }
        [HttpPost]
        public async Task<IActionResult> Create(RoleCreateDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tenantId = User.FindFirst("TenantId")?.Value;

            if (int.TryParse(tenantId, out var parsedTenantId))
                model.TenantId = parsedTenantId;

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token());

            var content = new StringContent(
                JsonConvert.SerializeObject(model),
                Encoding.UTF8,
                "application/json");

            var response = await http.PostAsync(ApiUrl("Role/CreateRole"), content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "خطا در ثبت نقش");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var json = await _getListApi.GetApiList(ApiUrl($"Role/GetRoleById?roleId={id}"));

            var result = JsonConvert.DeserializeObject<ApiResponse<RoleModel>>(json);
            var role = result?.Data;

            if (role == null)
                return NotFound();

            var model = new RoleEditDto
            {
                Id = role.Id,
                Name = role.Name,
                TenantId = role.TenantId
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(RoleEditDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token());

            var content = new StringContent(
                JsonConvert.SerializeObject(model),
                Encoding.UTF8,
                "application/json");

            var response = await http.PutAsync(ApiUrl("Role/UpdateRole"), content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "خطا در ویرایش نقش");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token());

            await http.DeleteAsync(ApiUrl($"Role/DeleteRole/{id}"));

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> ManagePermissions(string id)
        {
            var roleJson = await _getListApi.GetApiList(ApiUrl($"Role/GetRoleById?roleId={id}"));
            var roleResult = JsonConvert.DeserializeObject<ApiResponse<RoleModel>>(roleJson);
            var role = roleResult?.Data;

            if (role == null)
                return NotFound();

            var permJson = await _getListApi.GetApiList(ApiUrl("Role/GetPermissions"));
            var permResult = JsonConvert.DeserializeObject<ApiResponse<List<PermissionDto>>>(permJson);
            var allPermissions = permResult?.Data ?? new List<PermissionDto>();

            var selectedJson = await _getListApi.GetApiList(ApiUrl($"Role/GetRolePermissions?roleId={id}"));
            var selectedResult = JsonConvert.DeserializeObject<ApiResponse<List<int>>>(selectedJson);
            var selectedPermissionIds = selectedResult?.Data ?? new List<int>();

            var check = allPermissions.Select(x => new { x.Name, x.Category, x.CategoryTitleFa }).ToList();

            var model = new ManageRolePermissionsDto
            {
                RoleId = role.Id,
                RoleName = role.Name,
                SelectedPermissionIds = selectedPermissionIds,
                Permissions = allPermissions.Select(permission => new PermissionCheckboxDto
                {
                    PermissionId = permission.Id,
                    Name = permission.Name,
                    Title = permission.Title,
                    CategoryTitleFa = permission.CategoryTitleFa,
                    Category = permission.Category,
                    IsSelected = selectedPermissionIds.Contains(permission.Id)
                }).ToList()
            };
            
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ManagePermissions(ManageRolePermissionsDto model)
        {
            var updateModel = new UpdateRolePermissionsDto
            {
                RoleId = model.RoleId,
                PermissionIds = model.SelectedPermissionIds ?? new List<int>()
            };

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token());

            var content = new StringContent(
                JsonConvert.SerializeObject(updateModel),
                Encoding.UTF8,
                "application/json");

            var response = await http.PostAsync(ApiUrl("Role/UpdateRolePermissions"), content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "خطا در ذخیره دسترسی‌ها");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
