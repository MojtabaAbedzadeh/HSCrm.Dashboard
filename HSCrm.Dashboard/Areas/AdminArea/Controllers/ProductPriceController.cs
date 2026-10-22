using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    public class ProductPriceController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;

        public ProductPriceController(IConfiguration config, GetListApi getListApi):base(config)
        {
            _config = config;
            _getListApi = getListApi;
        }

        public async Task<IActionResult> Index()
        {
            string apiUrl = "ProductPrice/GetProductPriceHistory";
            string token = User.FindFirstValue("Token");

            try
            {
                var json = await _getListApi.GetApiList(apiUrl);
                if (string.IsNullOrWhiteSpace(json))
                {
                    ViewBag.ErrorMessage = "امکان دریافت تاریخچه قیمت‌ها وجود ندارد.";
                    return View(new List<ProductPriceModel>());
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<List<ProductPriceModel>>>(json);
                if (result == null || !result.Status)
                {
                    ViewBag.ErrorMessage = result?.Message ?? "خطا در دریافت تاریخچه قیمت‌ها.";
                    return View(new List<ProductPriceModel>());
                }

                var model = result.Data ?? new List<ProductPriceModel>();
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطا در ارتباط با سرور: " + ex.Message;
                return View(new List<ProductPriceModel>());
            }
        }
    }
}
