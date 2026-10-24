using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    public class SupplierController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;

        public SupplierController(IConfiguration config, GetListApi getListApi) : base(config) 
        {
            _config = config;
            _getListApi = getListApi;
        }

        public async Task<IActionResult> Index()
        {
            string apiUrl = "Supplier/GetSupplier";
            try
            {
                var json = await _getListApi.GetApiList(apiUrl);
                if (string.IsNullOrWhiteSpace(json))
                {
                    ViewBag.ErrorMessage = "پاسخی از سرویس تأمین‌کنندگان دریافت نشد.";
                    return View(new List<SupplierModel>());
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<List<SupplierModel>>>(json);
                if (result == null || !result.Status)
                {
                    ViewBag.ErrorMessage = result?.Message ?? "خطا در دریافت لیست تأمین‌کنندگان.";
                    return View(new List<SupplierModel>());
                }

                var model = result.Data ?? new List<SupplierModel>();
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطا در ارتباط با سرور: " + ex.Message;
                return View(new List<SupplierModel>());
            }
        }
    }
}
