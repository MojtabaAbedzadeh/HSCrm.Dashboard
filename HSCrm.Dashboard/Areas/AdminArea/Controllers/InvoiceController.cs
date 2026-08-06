using HSCrm.BussinessLogic.PublicMethod;
using HSCrm.Dashboard.Controllers;
using HSCrm.Models.Common;
using HSCrm.Models.ModelDto;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace HSCrm.Dashboard.Areas.AdminArea.Controllers
{
    [Area(nameof(AdminArea))]
    public class InvoiceController : BaseController
    {
        private readonly IConfiguration _config;
        private readonly GetListApi _getListApi;

        public InvoiceController(IConfiguration config, GetListApi getListApi) : base(config)
        {
            _getListApi = getListApi;
        }
        public async Task<IActionResult> SalesInvoice()
        {
            string apiUrl = "SalesInvoice/GetSalesInvoices";
            string token = User.FindFirstValue("Token");

            var json = await _getListApi.GetApiList(apiUrl);

            var result = JsonConvert.DeserializeObject<ApiResponse<List<SalesInvoiceModel>>>(json);
            var model = result.Data;

            return View(model);
        }
        public async Task<IActionResult> AddSaleInvoice()
        {
            var tenantId = User.FindFirstValue("TenantId");
            ViewBag.Projects = await GetProjectList(tenantId);
            ViewBag.Warehouses = await GetWarehouseList(tenantId);

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> PurchaseInvoice(int? supplierId = null, DateTime? fromDate = null, DateTime? toDate = null, decimal? minTotal = null, decimal? maxTotal = null)
        {
            var queryParameters = new List<string>();

            if (supplierId.HasValue)
            {
                queryParameters.Add($"supplierId={Uri.EscapeDataString(supplierId.Value.ToString())}");
            }

            if (fromDate.HasValue)
            {
                queryParameters.Add($"fromDate={Uri.EscapeDataString(fromDate.Value.ToString("yyyy-MM-dd"))}");
            }

            if (toDate.HasValue)
            {
                queryParameters.Add($"toDate={Uri.EscapeDataString(toDate.Value.ToString("yyyy-MM-dd"))}");
            }

            if (minTotal.HasValue)
            {
                queryParameters.Add($"minTotal={Uri.EscapeDataString(minTotal.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))}");
            }

            if (maxTotal.HasValue)
            {
                queryParameters.Add($"maxTotal={Uri.EscapeDataString(maxTotal.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))}");
            }

            var apiUrl = "PurchaseInvoice/GetPurchaseInvoices";

            if (queryParameters.Count > 0)
            {
                apiUrl += "?" + string.Join("&", queryParameters);
            }

            var json = await _getListApi.GetApiList(apiUrl);

            var result = JsonConvert.DeserializeObject<ApiResponse<List<PurchaseInvoiceModel>>>(json);

            var model = result?.Data ?? new List<PurchaseInvoiceModel>();

            return View(model);
        }

        public async Task<IActionResult> AddPurchaseInvoice()
        {
            var tenantId = User.FindFirstValue("TenantId");
            ViewBag.Projects = await GetProjectList(tenantId);
            ViewBag.Suppliers = await GetSupplierList(tenantId);
            ViewBag.Warehouses = await GetWarehouseList(tenantId);

            return View();
        }
        [HttpGet]
        private async Task<List<ProjetcsDropDown>> GetProjectList(string tenantId)
        {
            string apiUrlProject = "Project/ProjectDropdownList?tenantId=" + tenantId;
            string token = User.FindFirstValue("Token");

            string json = await _getListApi.GetApiList(apiUrlProject);

            var parsed = JObject.Parse(json);
            return JsonConvert.DeserializeObject<List<ProjetcsDropDown>>(
                parsed["data"]?.ToString() ?? "[]"
            );
        }
        [HttpGet]
        private async Task<List<SuppliersDropDown>> GetSupplierList(string tenantId)
        {
            string apiUrlSupplier = "Supplier/SupplierDropdownList?tenantId=" + tenantId;
            string token = User.FindFirstValue("Token");

            string json = await _getListApi.GetApiList(apiUrlSupplier);

            var parsed = JObject.Parse(json);
            return JsonConvert.DeserializeObject<List<SuppliersDropDown>>(
                parsed["data"]?.ToString() ?? "[]"
            );
        }
        [HttpGet]
        private async Task<List<WarehousesDropDown>> GetWarehouseList(string tenantId)
        {
            string apiUrlWarehouse = "Warehouse/WarehouseDropdownList?tenantId=" + tenantId;
            string token = User.FindFirstValue("Token");

            string json = await _getListApi.GetApiList(apiUrlWarehouse);

            var parsed = JObject.Parse(json);
            return JsonConvert.DeserializeObject<List<WarehousesDropDown>>(
                parsed["data"]?.ToString() ?? "[]"
            );
        }        
        [HttpGet]
        public async Task<IActionResult> ProjectInvoices(int projectId)
        {
            string apiUrl = "SalesInvoice/GetByProjectId?projectId=" + projectId;
            string token = User.FindFirstValue("Token");

            var json = await _getListApi.GetApiList(apiUrl);
            var result = JsonConvert.DeserializeObject<ApiResponse<List<SalesInvoiceModel>>>(json);
            var model = result.Data;

            var tenantId = User.FindFirstValue("TenantId");

            return View(model);
        }
    }
}
