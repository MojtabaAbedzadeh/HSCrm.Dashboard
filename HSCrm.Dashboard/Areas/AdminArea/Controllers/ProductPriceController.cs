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
    public class ProductPriceController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;

        public ProductPriceController(IConfiguration config, GetListApi getListApi):base(config)
        {
            _getListApi = getListApi;
        }
        public async Task<IActionResult> Index()
        {
            string apiUrl = _config["ApiAddress"] + "ProductPrice/GetProductPriceHistory";
            string token = User.FindFirstValue("Token");

            var json = await _getListApi.GetApiList(apiUrl);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<ProductPriceModel>>>(json);
            var model = result.Data;

            return View(model);
        }
    }
}
