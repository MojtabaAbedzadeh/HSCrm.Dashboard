using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using Microsoft.AspNetCore.Mvc;
using HSCrm.Dashboard.Costants;
using Microsoft.AspNetCore.Authorization;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    public class ReturnInvoiceController : BaseController
    {
        private readonly GetListApi _getListApi;

        public ReturnInvoiceController(IConfiguration config, GetListApi getListApi) : base(config)
        {
            _getListApi = getListApi;
        }

        [HttpGet]
        [Authorize(Policy = "Permission:" + Permissions.SalesReturnInvoices.View)]
        public IActionResult SalesReturn()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:" + Permissions.PurchaseReturnInvoices.View)]
        public IActionResult PurchaseReturn()
        {
            return View();
        }
    }
}
