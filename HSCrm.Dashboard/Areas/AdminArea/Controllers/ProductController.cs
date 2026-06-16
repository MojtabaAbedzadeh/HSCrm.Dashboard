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
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IConfiguration _config;

        public ProductController(IConfiguration config)
        {
            _config = config;
        }
        public async Task<IActionResult> Index()
        {
            string apiUrl = _config["ApiAddress"] + "Product/GetProducts";
            string token = User.FindFirstValue("Token");
            GetListApi GA = new GetListApi();
            var json = await GA.GetApiList(apiUrl, token);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<ProductModel>>>(json);
            var model = result.Data;
            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.Token = User.FindFirstValue("Token");
            ViewBag.FirstName = User.FindFirstValue("FirstName");
            ViewBag.LastName = User.FindFirstValue("LastName");
            ViewBag.TenantId = User.FindFirstValue("TenantId");
            ViewBag.UserImageUrl = User.FindFirstValue("UserImageUrl");
            ViewBag.FiscalYearStatus = User.FindFirstValue("FiscalYearStatus");
            return View(model);
        }
    }
}
