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
            try
            {
                var json = await _getListApi.GetApiList(apiUrl);
                if (string.IsNullOrWhiteSpace(json))
                {
                    ViewBag.ErrorMessage = "پاسخی از سرور مشتریان دریافت نشد.";
                    return View(new List<CustomerModel>());
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<List<CustomerModel>>>(json);
                var model = result?.Data ?? new List<CustomerModel>();
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطا در بازیابی لیست مشتریان: " + ex.Message;
                return View(new List<CustomerModel>());
            }
        }

        public async Task<IActionResult> CustomerProfile(int customerId)
        {
            string apiUrl = $"Customer/GetCustomerById?customerId={customerId}";
            try
            {
                var json = await _getListApi.GetApiList(apiUrl);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return RedirectToAction(nameof(Index));
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<CustomerProfileViewModel>>(json);
                var model = result?.Data;
                if (model == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return View(model);
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
