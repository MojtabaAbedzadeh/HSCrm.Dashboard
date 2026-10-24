using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    [Authorize(Roles = "Owner")]
    public class UserController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;

        public UserController(IConfiguration config, GetListApi getListApi) : base(config)
        {
            _getListApi = getListApi;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var usersTask = _getListApi.GetApiList("User/GetUsers");
                var rolesTask = _getListApi.GetApiList("Role/GetRoles");
                await Task.WhenAll(usersTask, rolesTask);

                var usersJson = await usersTask;
                var rolesJson = await rolesTask;

                var userResult = !string.IsNullOrWhiteSpace(usersJson) 
                    ? JsonConvert.DeserializeObject<ApiResponse<List<UserModel>>>(usersJson) 
                    : null;
                var roleResult = !string.IsNullOrWhiteSpace(rolesJson) 
                    ? JsonConvert.DeserializeObject<ApiResponse<List<RoleModel>>>(rolesJson) 
                    : null;

                ViewBag.Roles = roleResult?.Data ?? new List<RoleModel>();
                return View(userResult?.Data ?? new List<UserModel>());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطا در دریافت لیست کاربران: " + ex.Message;
                ViewBag.Roles = new List<RoleModel>();
                return View(new List<UserModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string roleId)
        {
            try
            {
                string apiUrl = $"User/AssignRole?userId={userId}&roleId={roleId}";
                await _getListApi.GetApiList(apiUrl);
            }
            catch
            {
                // لاگ خطا در صورت بروز
            }
            return RedirectToAction("Index");
        }
    }
}
