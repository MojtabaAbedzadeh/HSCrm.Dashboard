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
    public class ProductController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;

        public ProductController(IConfiguration config, GetListApi getListApi) : base(config) 
        {
            _getListApi = getListApi;
        }
        public async Task<IActionResult> Index()
        {
            string apiUrl = "Product/GetProducts";
            string token = User.FindFirstValue("Token");

            var json = await _getListApi.GetApiList(apiUrl);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<ProductModel>>>(json);
            var model = result.Data;

            return View(model);
        }
    }
}
