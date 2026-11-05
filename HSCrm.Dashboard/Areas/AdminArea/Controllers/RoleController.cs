using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    [Authorize(Roles = "Owner")]
    public class RoleController : BaseController
    {
        private readonly GetListApi _getListApi;
        private readonly IHttpClientFactory _httpClientFactory;

        public RoleController(
            IConfiguration config,
            GetListApi getListApi,
            IHttpClientFactory httpClientFactory) : base(config)
        {
            _getListApi = getListApi;
            _httpClientFactory = httpClientFactory;
        }

        private string ApiUrl(string endpoint)
        {
            return _config["ApiAddress"] + endpoint;
        }

        private HttpClient CreateApiClient()
        {
            return _httpClientFactory.CreateClient("ApiClient");
        }

        private async Task<(bool IsSuccess, string Message)> ReadApiResult(
            HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            ApiResponse<object>? result = null;

            if (!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    result = JsonConvert.DeserializeObject<ApiResponse<object>>(json);
                }
                catch
                {
                    // Invalid response JSON is handled below.
                }
            }

            var isSuccess = response.IsSuccessStatusCode &&
                            result?.Status == true;

            var message = !string.IsNullOrWhiteSpace(result?.Message)
                ? result.Message
                : response.IsSuccessStatusCode
                    ? "پاسخ معتبر از سرویس دریافت نشد."
                    : $"خطا در ارتباط با سرویس (HTTP {(int)response.StatusCode}).";

            return (isSuccess, message);
        }
        private ApiResponse<T>? DeserializeApiResponse<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<ApiResponse<T>>(json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private string GetApiErrorMessage<T>(ApiResponse<T>? result)
        {
            return !string.IsNullOrWhiteSpace(result?.Message)
                ? result.Message
                : "دریافت اطلاعات از سرویس با مشکل مواجه شد.";
        }
        public async Task<IActionResult> Index()
        {
            var result = DeserializeApiResponse<List<RoleModel>>(
                await _getListApi.GetApiList(ApiUrl("Role/GetRoles")));

            if (result?.Status != true || result.Data == null)
            {
                TempData["Error"] = GetApiErrorMessage(result);
                return View(new List<RoleModel>());
            }

            return View(result.Data);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View(new RoleCreateDto());
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(RoleCreateDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var tenantId = User.FindFirst("TenantId")?.Value;

            if (int.TryParse(tenantId, out var parsedTenantId))
                model.TenantId = parsedTenantId;

            var http = CreateApiClient();

            var response = await http.PostAsJsonAsync(
                "Role/CreateRole",
                model);

            var apiResult = await ReadApiResult(response);

            if (!apiResult.IsSuccess)
            {
                ModelState.AddModelError("", apiResult.Message);
                return View(model);
            }

            TempData["Success"] = "نقش با موفقیت ثبت شد.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var safeId = Uri.EscapeDataString(id);

            var result = DeserializeApiResponse<RoleModel>(
                await _getListApi.GetApiList(
                    ApiUrl($"Role/GetRoleById?roleId={safeId}")));

            if (result?.Status != true)
            {
                TempData["Error"] = GetApiErrorMessage(result);
                return RedirectToAction(nameof(Index));
            }

            var role = result.Data;

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


        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(RoleEditDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var http = CreateApiClient();

            var response = await http.PutAsJsonAsync(
                "Role/UpdateRole",
                model);

            var apiResult = await ReadApiResult(response);

            if (!apiResult.IsSuccess)
            {
                ModelState.AddModelError("", apiResult.Message);
                return View(model);
            }

            TempData["Success"] = "نقش با موفقیت ویرایش شد.";

            return RedirectToAction(nameof(Index));
        }

        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Error"] = "شناسه نقش معتبر نیست.";
                return RedirectToAction(nameof(Index));
            }

            var http = CreateApiClient();

            var safeId = Uri.EscapeDataString(id);

            var response = await http.DeleteAsync(
                $"Role/DeleteRole?roleId={safeId}");

            var apiResult = await ReadApiResult(response);

            if (!apiResult.IsSuccess)
            {
                TempData["Error"] = apiResult.Message;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "نقش با موفقیت حذف شد.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ManagePermissions(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var safeId = Uri.EscapeDataString(id);

            var roleResult = DeserializeApiResponse<RoleModel>(
                await _getListApi.GetApiList(
                    ApiUrl($"Role/GetRoleById?roleId={safeId}")));

            if (roleResult?.Status != true)
            {
                TempData["Error"] = GetApiErrorMessage(roleResult);
                return RedirectToAction(nameof(Index));
            }

            var role = roleResult.Data;

            if (role == null)
                return NotFound();

            var permResult = DeserializeApiResponse<List<PermissionDto>>(
                await _getListApi.GetApiList(ApiUrl("Role/GetPermissions")));

            if (permResult?.Status != true || permResult.Data == null)
            {
                TempData["Error"] = GetApiErrorMessage(permResult);
                return RedirectToAction(nameof(Index));
            }

            var selectedResult = DeserializeApiResponse<List<int>>(
                await _getListApi.GetApiList(
                    ApiUrl($"Role/GetRolePermissions?roleId={safeId}")));

            if (selectedResult?.Status != true || selectedResult.Data == null)
            {
                TempData["Error"] = GetApiErrorMessage(selectedResult);
                return RedirectToAction(nameof(Index));
            }

            var allPermissions = permResult.Data;
            var selectedPermissionIds = selectedResult.Data;

            var model = new ManageRolePermissionsDto
            {
                RoleId = role.Id,
                RoleName = role.Name,
                SelectedPermissionIds = selectedPermissionIds,
                Permissions = allPermissions.Select(permission =>
                    new PermissionCheckboxDto
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


        [AutoValidateAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> ManagePermissions(
            ManageRolePermissionsDto model)
        {
            var updateModel = new UpdateRolePermissionsDto
            {
                RoleId = model.RoleId,
                PermissionIds = model.SelectedPermissionIds ?? new List<int>()
            };

            var http = CreateApiClient();

            var response = await http.PostAsJsonAsync(
                "Role/UpdateRolePermissions",
                updateModel);

            var apiResult = await ReadApiResult(response);
            if (!apiResult.IsSuccess)
            {
                return BadRequest(new { message = apiResult.Message });
            }

            return Ok(new { message = apiResult.Message });
        }
    }
}