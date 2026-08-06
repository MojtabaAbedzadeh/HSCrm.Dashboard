using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    public class PaymentController : BaseController
    {
        private readonly GetListApi _getListApi;

        public PaymentController(IConfiguration config, GetListApi getListApi) : base(config)
        {
            _getListApi = getListApi;
        }

        public async Task<IActionResult> Index()
        {
            string apiUrl = "Payment/GetPayments";

            try
            {
                var json = await _getListApi.GetApiList(apiUrl);

                if (string.IsNullOrWhiteSpace(json))
                {
                    ViewBag.ErrorMessage = $"پاسخ خالی از API دریافت شد. آدرس: {apiUrl}";
                    return View(new List<PaymentModel>());
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<List<PaymentModel>>>(json);

                if (result == null)
                {
                    ViewBag.ErrorMessage = $"JSON معتبر نیست. خروجی خام: {json}";
                    return View(new List<PaymentModel>());
                }

                return View(result.Data ?? new List<PaymentModel>());
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"خطا هنگام دریافت پرداخت‌ها: {ex.Message}";
                return View(new List<PaymentModel>());
            }
        }

    }
}
