using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authorization;
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

            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.Token = token;
            ViewBag.FirstName = User.FindFirstValue("FirstName");

            return View(model);
        }
    }
}
