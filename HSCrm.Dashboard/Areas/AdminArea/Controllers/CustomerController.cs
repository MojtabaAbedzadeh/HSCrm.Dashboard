using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]    
    public class CustomerController : Controller
    {
        private readonly IConfiguration _config;

        public CustomerController(IConfiguration config)
        {
            _config = config;
        }
        public async Task<IActionResult> Index()
        {
            string apiUrl = _config["ApiAddress"] + "Customer/GetCustomer";
            string token = User.FindFirstValue("Token");

            GetListApi GA = new GetListApi();
            var json = await GA.GetApiList(apiUrl, token);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<CustomerModel>>>(json);
            var model = result.Data;

            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.Token = User.FindFirstValue("Token");
            ViewBag.FirstName = User.FindFirstValue("FirstName");
            ViewBag.LastName = User.FindFirstValue("LastName");
            ViewBag.UserImageUrl = User.FindFirstValue("UserImageUrl");

            return View(model);
        }

        public async Task<IActionResult> CustomerProfile(int customerId)
        {
            string apiUrl = _config["ApiAddress"] + "Customer/GetCustomerById?customerId=" + customerId;
            string token = User.FindFirstValue("Token");

            GetListApi GA = new GetListApi();
            var json = await GA.GetApiList(apiUrl, token);
            var result = JsonConvert.DeserializeObject<ApiResponse<CustomerDetailViewModel>>(json);
            var model = result.Data;

            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.Token = User.FindFirstValue("Token");
            ViewBag.FirstName = User.FindFirstValue("FirstName");
            ViewBag.LastName = User.FindFirstValue("LastName");
            ViewBag.UserImageUrl = User.FindFirstValue("UserImageUrl");

            return View(model);
        }
    }
}
