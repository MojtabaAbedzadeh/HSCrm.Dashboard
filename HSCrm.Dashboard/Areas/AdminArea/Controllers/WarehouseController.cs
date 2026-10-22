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
    public class WarehouseController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;

        public WarehouseController(IConfiguration config, GetListApi getListApi) : base(config) 
        {
            _config = config;
            _getListApi = getListApi;
        }

        public async Task<IActionResult> Index()
        {
            // فراخوانی با روت نسبی سازگار با ApiClient BaseAddress
            string apiUrl = "Warehouse/GetWarehouses";
            string token = User.FindFirstValue("Token");

            try
            {
                var json = await _getListApi.GetApiList(apiUrl);
                if (string.IsNullOrWhiteSpace(json))
                {
                    ViewBag.ErrorMessage = "هیچ پاسخی از سرویس انبارها دریافت نشد یا دسترسی مجاز نیست.";
                    return View(new List<WarehousesModel>());
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<List<WarehousesModel>>>(json);
                if (result == null || !result.Status)
                {
                    ViewBag.ErrorMessage = result?.Message ?? "خطا در دریافت لیست انبارها از سرور.";
                    return View(new List<WarehousesModel>());
                }

                var model = result.Data ?? new List<WarehousesModel>();
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطای غیرمنتظره در ارتباط با سرور: " + ex.Message;
                return View(new List<WarehousesModel>());
            }
        }
    }
}
