using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    public class CustomerController : BaseController
    {
        private readonly GetListApi _getListApi; 

        public CustomerController(IConfiguration config, GetListApi getListApi) : base(config)
        {
            _getListApi = getListApi;
        }

        public async Task<IActionResult> Index()
        {
            string apiUrl = "Customer/GetCustomer";

            var json = await _getListApi.GetApiList(apiUrl);

            var result = JsonConvert.DeserializeObject<ApiResponse<List<CustomerModel>>>(json);
            var model = result?.Data;

            return View(model);
        }

        public async Task<IActionResult> CustomerProfile(int customerId)
        {
            string apiUrl = $"Customer/GetCustomerById?customerId={customerId}";

            var json = await _getListApi.GetApiList(apiUrl);

            var result = JsonConvert.DeserializeObject<ApiResponse<CustomerProfileViewModel>>(json);
            var model = result?.Data;

            return View(model);
        }
    }
}