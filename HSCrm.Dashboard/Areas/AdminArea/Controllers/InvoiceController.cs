using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    [Authorize(Roles = "Admin")]
    public class InvoiceController : Controller
    {
        private readonly IConfiguration _config;
        public InvoiceController(IConfiguration config)
        {
            _config = config;
        }
        public async Task<IActionResult> SalesInvoice()
        {
            string apiUrl = _config["ApiAddress"] + "SalesInvoice/GetSalesInvoices";
            string token = User.FindFirstValue("Token");
            GetListApi GA = new GetListApi();
            var json = await GA.GetApiList(apiUrl, token);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<SalesInvoiceModel>>>(json);
            var model = result.Data;
            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.TenantId = User.FindFirstValue("TenantId");
            ViewBag.Token = User.FindFirstValue("Token");
            ViewBag.FirstName = User.FindFirstValue("FirstName");
            ViewBag.LastName = User.FindFirstValue("LastName");
            ViewBag.UserImageUrl = User.FindFirstValue("UserImageUrl");
            ViewBag.FiscalYearStatus = User.FindFirstValue("FiscalYearStatus");
            return View(model);
        }
        public async Task<IActionResult> AddSaleInvoice()
        {
            var tenantId = User.FindFirstValue("TenantId");
            ViewBag.Projects = await GetProjectList(tenantId);
            ViewBag.Warehouses = await GetWarehouseList(tenantId);

            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.TenantId = User.FindFirstValue("TenantId");
            ViewBag.Token = User.FindFirstValue("Token");
            ViewBag.FirstName = User.FindFirstValue("FirstName");
            ViewBag.LastName = User.FindFirstValue("LastName");
            ViewBag.UserImageUrl = User.FindFirstValue("UserImageUrl");
            ViewBag.FiscalYearStatus = User.FindFirstValue("FiscalYearStatus");
            return View();
        }
        public async Task<IActionResult> PurchaseInvoice()
        {
            string apiUrl = _config["ApiAddress"] + "PurchaseInvoice/GetPurchaseInvoices";
            string token = User.FindFirstValue("Token");
            GetListApi GA = new GetListApi();
            var json = await GA.GetApiList(apiUrl, token);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<PurchaseInvoiceModel>>>(json);
            var model = result.Data;
            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.TenantId = User.FindFirstValue("TenantId");
            ViewBag.Token = User.FindFirstValue("Token");
            ViewBag.FirstName = User.FindFirstValue("FirstName");
            ViewBag.LastName = User.FindFirstValue("LastName");
            ViewBag.UserImageUrl = User.FindFirstValue("UserImageUrl");
            ViewBag.FiscalYearStatus = User.FindFirstValue("FiscalYearStatus");
            return View(model);
        }
        public async Task<IActionResult> AddPurchaseInvoice()
        {
            var tenantId = User.FindFirstValue("TenantId");
            ViewBag.Projects = await GetProjectList(tenantId);
            ViewBag.Suppliers = await GetSupplierList(tenantId);
            ViewBag.Warehouses = await GetWarehouseList(tenantId);

            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.TenantId = User.FindFirstValue("TenantId");
            ViewBag.Token = User.FindFirstValue("Token");
            ViewBag.FirstName = User.FindFirstValue("FirstName");
            ViewBag.LastName = User.FindFirstValue("LastName");
            ViewBag.UserImageUrl = User.FindFirstValue("UserImageUrl");
            ViewBag.FiscalYearStatus = User.FindFirstValue("FiscalYearStatus");

            return View();
        }
        [HttpGet]
        private async Task<List<ProjetcsDropDown>> GetProjectList(string tenantId)
        {
            string apiUrlProject = _config["ApiAddress"] + "Project/ProjectDropdownList?tenantId=" + tenantId;
            string token = User.FindFirstValue("Token");
            GetListApi getListProject = new GetListApi();
            string json = await getListProject.GetApiList(apiUrlProject, token);

            var parsed = JObject.Parse(json);
            return JsonConvert.DeserializeObject<List<ProjetcsDropDown>>(
                parsed["data"]?.ToString() ?? "[]"
            );
        }
        [HttpGet]
        private async Task<List<SuppliersDropDown>> GetSupplierList(string tenantId)
        {
            string apiUrlSupplier = _config["ApiAddress"] + "Supplier/SupplierDropdownList?tenantId=" + tenantId;
            string token = User.FindFirstValue("Token");
            GetListApi getListSupplier = new GetListApi();
            string json = await getListSupplier.GetApiList(apiUrlSupplier, token);

            var parsed = JObject.Parse(json);
            return JsonConvert.DeserializeObject<List<SuppliersDropDown>>(
                parsed["data"]?.ToString() ?? "[]"
            );
        }
        [HttpGet]
        private async Task<List<WarehousesDropDown>> GetWarehouseList(string tenantId)
        {
            string apiUrlWarehouse = _config["ApiAddress"] + "Warehouse/WarehouseDropdownList?tenantId=" + tenantId;
            string token = User.FindFirstValue("Token");
            GetListApi getListWarehouse = new GetListApi();
            string json = await getListWarehouse.GetApiList(apiUrlWarehouse, token);

            var parsed = JObject.Parse(json);
            return JsonConvert.DeserializeObject<List<WarehousesDropDown>>(
                parsed["data"]?.ToString() ?? "[]"
            );
        }
        
        [HttpGet]
        public async Task<IActionResult> ProjectInvoices(int projectId)
        {
            string apiUrl = _config["ApiAddress"] + "SalesInvoice/GetByProjectId?projectId=" + projectId;
            string token = User.FindFirstValue("Token");
            GetListApi GA = new GetListApi();
            var json = await GA.GetApiList(apiUrl, token);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<SalesInvoiceModel>>>(json);
            var model = result.Data;

            var tenantId = User.FindFirstValue("TenantId");

            ViewBag.ApiAddress = _config["ApiAddress"];
            ViewBag.TenantId = User.FindFirstValue("TenantId");
            ViewBag.UserId = User.FindFirstValue("UserId");
            ViewBag.Token = User.FindFirstValue("Token");
            ViewBag.FirstName = User.FindFirstValue("FirstName");
            ViewBag.LastName = User.FindFirstValue("LastName");
            ViewBag.UserImageUrl = User.FindFirstValue("UserImageUrl");
            ViewBag.FiscalYearStatus = User.FindFirstValue("FiscalYearStatus");

            return View(model);
        }
    }
}
