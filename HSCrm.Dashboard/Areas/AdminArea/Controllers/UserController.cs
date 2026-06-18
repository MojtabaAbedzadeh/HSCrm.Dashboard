using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    public class UserController : Controller
    {
        private readonly IConfiguration _config;
        public UserController(IConfiguration config)
        {
            _config = config;
        }
        public async Task<IActionResult> Index()
        {
            string apiUrl = _config["ApiAddress"] + "User/GetUsers";
            string token = User.FindFirstValue("Token");

            GetListApi GA = new GetListApi();
            var json = await GA.GetApiList(apiUrl, token);

            var result = JsonConvert.DeserializeObject<ApiResponse<List<UserModel>>>(json);
            var model = result.Data;

            // گرفتن نقش ها
            string roleUrl = _config["ApiAddress"] + "Role/GetRoles";
            var roles = await GA.GetApiList(roleUrl, token);
            var roleResult = JsonConvert.DeserializeObject<ApiResponse<List<RoleModel>>>(roles);

            // ارسال به View
            ViewBag.Roles = roleResult.Data;

            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.Token = token;
            ViewBag.FirstName = User.FindFirstValue("FirstName");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string roleId)
        {
            string apiUrl = _config["ApiAddress"] + $"User/AssignRole?userId={userId}&roleId={roleId}";
            string token = User.FindFirstValue("Token");
            if (string.IsNullOrEmpty(token))
            {
                return Content("Token is null or empty");
            }
            GetListApi GA = new GetListApi();
            await GA.GetApiList(apiUrl, token);

            return RedirectToAction("Index");
        }
    }
}
