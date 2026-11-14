using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using Microsoft.AspNetCore.Mvc;
using HSCrm.Dashboard.Costants;
using Microsoft.AspNetCore.Authorization;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    public class AccountingController : BaseController
    {
        private readonly GetListApi _getListApi;

        public AccountingController(IConfiguration config, GetListApi getListApi) : base(config)
        {
            _getListApi = getListApi;
        }

        [HttpGet]
        [Authorize(Policy = "Permission:" + Permissions.AccountingAccounts.View)]
        public IActionResult Accounts()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:" + Permissions.JournalVouchers.View)]
        public IActionResult Vouchers()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:" + Permissions.AccountingReports.View)]
        public IActionResult Reports(int? accountId = null, int? bankAccountId = null, string tab = "trial-balance")
        {
            ViewBag.InitialAccountId = accountId;
            ViewBag.InitialBankAccountId = bankAccountId;
            ViewBag.InitialTab = tab;
            return View();
        }

        [HttpGet]
        [Authorize(Policy = "Permission:" + Permissions.AccountingReports.View)]
        public IActionResult AgingAndStatements(int partyType = 1, int? partyId = null)
        {
            ViewBag.InitialPartyType = partyType;
            ViewBag.InitialPartyId = partyId;
            return View();
        }
    }
}
