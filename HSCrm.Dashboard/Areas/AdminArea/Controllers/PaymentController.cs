using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    public class PaymentController : BaseController
    {
        private readonly GetListApi _getListApi; // اضافه کردن فیلد برای GetListApi

        public PaymentController(IConfiguration config, GetListApi getListApi) : base(config)
        {
            _getListApi = getListApi;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
