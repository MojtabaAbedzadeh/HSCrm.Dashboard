using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

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
            string apiUrl = "User/GetUsers";

            var json = await _getListApi.GetApiList(apiUrl);

            var result = JsonConvert.DeserializeObject<ApiResponse<List<UserModel>>>(json);
            var model = result.Data;

            string roleUrl = "Role/GetRoles";
            var roles = await _getListApi.GetApiList(roleUrl);

            var roleResult = JsonConvert.DeserializeObject<ApiResponse<List<RoleModel>>>(roles);

            ViewBag.Roles = roleResult.Data;
      
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string roleId)
        {
            string apiUrl = $"User/AssignRole?userId={userId}&roleId={roleId}";
            await _getListApi.GetApiList(apiUrl);

            return RedirectToAction("Index");
        }
    }
}
