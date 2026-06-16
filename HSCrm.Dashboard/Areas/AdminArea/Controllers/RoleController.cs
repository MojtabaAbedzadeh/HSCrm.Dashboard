using HSCrm.Dashboard.Services.Interface;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    [Authorize]
    public class RoleController : Controller
    {
        private readonly IRoleApiService _roleApiService;

        public RoleController(IRoleApiService roleApiService)
        {
            _roleApiService = roleApiService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _roleApiService.GetRoles();
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
            {
                return View(model);
            }

            var tenantId = User.FindFirst("TenantId")?.Value;

            if (int.TryParse(tenantId, out var parsedTenantId))
            {
                model.TenantId = parsedTenantId;
            }

            var result = await _roleApiService.CreateRole(model);

            if (!result)
            {
                ModelState.AddModelError("", "خطا در ثبت نقش");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleApiService.GetRoleById(id);

            if (role == null)
            {
                return NotFound();
            }

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
            {
                return View(model);
            }

            var result = await _roleApiService.UpdateRole(model);

            if (!result)
            {
                ModelState.AddModelError("", "خطا در ویرایش نقش");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _roleApiService.DeleteRole(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ManagePermissions(string id)
        {
            var role = await _roleApiService.GetRoleById(id);

            if (role == null)
            {
                return NotFound();
            }

            var allPermissions = await _roleApiService.GetPermissions();
            var selectedPermissionIds = await _roleApiService.GetRolePermissions(id);

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

            var result = await _roleApiService.UpdateRolePermissions(updateModel);

            if (!result)
            {
                ModelState.AddModelError("", "خطا در ذخیره دسترسی‌ها");

                var allPermissions = await _roleApiService.GetPermissions();

                model.Permissions = allPermissions.Select(permission => new PermissionCheckboxDto
                {
                    PermissionId = permission.Id,
                    Name = permission.Name,
                    Title = permission.Title,
                    Category = permission.Category,
                    IsSelected = model.SelectedPermissionIds.Contains(permission.Id)
                }).ToList();

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
