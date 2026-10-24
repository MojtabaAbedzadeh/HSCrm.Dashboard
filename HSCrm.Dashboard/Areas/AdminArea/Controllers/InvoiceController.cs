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
            _config = config;
            _getListApi = getListApi;
        }

        public async Task<IActionResult> SalesInvoice()
        {
            string apiUrl = "SalesInvoice/GetSalesInvoices";
            try
            {
                var json = await _getListApi.GetApiList(apiUrl);
                if (string.IsNullOrWhiteSpace(json))
                {
                    ViewBag.ErrorMessage = "پاسخی از سرور برای فاکتورهای فروش دریافت نشد.";
                    return View(new List<SalesInvoiceModel>());
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<List<SalesInvoiceModel>>>(json);
                var model = result?.Data ?? new List<SalesInvoiceModel>();
                if (result != null && !result.Status && !string.IsNullOrEmpty(result.Message))
                {
                    ViewBag.ErrorMessage = result.Message;
                }
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطا در دریافت فاکتورهای فروش: " + ex.Message;
                return View(new List<SalesInvoiceModel>());
            }
        }

        public async Task<IActionResult> AddSaleInvoice()
        {
            var tenantId = User.FindFirstValue("TenantId");
            ViewBag.Projects = await GetProjectList(tenantId);
            ViewBag.Warehouses = await GetWarehouseList(tenantId);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PurchaseInvoice(string supplierFullName = null, DateTime? fromDate = null, DateTime? toDate = null, decimal? minTotal = null, decimal? maxTotal = null)
        {
            var queryParameters = new List<string>();
            if (!string.IsNullOrEmpty(supplierFullName))
            {
                queryParameters.Add($"supplierFullName={Uri.EscapeDataString(supplierFullName)}");
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

            try
            {
                var json = await _getListApi.GetApiList(apiUrl);
                if (string.IsNullOrWhiteSpace(json))
                {
                    ViewBag.ErrorMessage = "پاسخی از سرور برای فاکتورهای خرید دریافت نشد.";
                    return View(new List<PurchaseInvoiceModel>());
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<List<PurchaseInvoiceModel>>>(json);
                var model = result?.Data ?? new List<PurchaseInvoiceModel>();
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطا در دریافت فاکتورهای خرید: " + ex.Message;
                return View(new List<PurchaseInvoiceModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditPurchaseInvoice(int id)
        {
            var tenantId = User.FindFirstValue("TenantId");
            string apiUrl = "PurchaseInvoice/GetById?InvoiceId=" + id;
            try
            {
                string json = await _getListApi.GetApiList(apiUrl);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return RedirectToAction(nameof(PurchaseInvoice));
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<PurInvoiceModel>>(json);
                if (result == null || !result.Status || result.Data == null)
                {
                    return RedirectToAction(nameof(PurchaseInvoice));
                }

                var model = new PurchaseInvoiceEditModel
                {
                    Id = result.Data.Id,
                    SupplierId = result.Data.SupplierId,
                    WarehouseId = result.Data.WarehouseId,
                    Number = result.Data.Number,
                    IssueDate = result.Data.IssueDate,
                    Tax = result.Data.Tax,
                    Discount = result.Data.Discount,
                    Total = result.Data.Total,
                    PaidAmount = result.Data.PaidAmount,
                    RemainingAmount = result.Data.Total - result.Data.PaidAmount,
                    Items = result.Data.Items?.Select(i => new PurchaseInvoiceItemModel
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPricePurchase = i.UnitPricePurchase != 0 ? i.UnitPricePurchase : i.UnitPrice,
                        UnitPrice = i.UnitPrice != 0 ? i.UnitPrice : i.UnitPricePurchase,
                        Discount = i.Discount,
                        Tax = i.Tax,
                        Product = i.Product != null ? new ProductModel
                        {
                            ProductId = i.ProductId,
                            ProductTitle = i.Product.ProductTitle,
                            ProductUnit = i.Product.ProductUnit,
                            TenantId = i.Product.TenantId
                        } : null
                    }).ToList() ?? new List<PurchaseInvoiceItemModel>()
                };

                ViewBag.Suppliers = await GetSupplierList(tenantId);
                ViewBag.Warehouses = await GetWarehouseList(tenantId);
                return View(model);
            }
            catch
            {
                return RedirectToAction(nameof(PurchaseInvoice));
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditPurchaseInvoice(PurchaseInvoiceModel model)
        {
            try
            {
                var result = await _getListApi.PostApi($"PurchaseInvoice/Update", model);
                return Json(new { success = true, message = "فاکتور خرید با موفقیت ویرایش شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در سیستم: " + ex.Message });
            }
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
            try
            {
                string apiUrlProject = "Project/ProjectDropdownList?tenantId=" + tenantId;
                string json = await _getListApi.GetApiList(apiUrlProject);
                if (string.IsNullOrWhiteSpace(json)) return new List<ProjetcsDropDown>();

                var parsed = JObject.Parse(json);
                return JsonConvert.DeserializeObject<List<ProjetcsDropDown>>(
                    parsed["data"]?.ToString() ?? "[]"
                ) ?? new List<ProjetcsDropDown>();
            }
            catch
            {
                return new List<ProjetcsDropDown>();
            }
        }

        [HttpGet]
        private async Task<List<SuppliersDropDown>> GetSupplierList(string tenantId)
        {
            try
            {
                string apiUrlSupplier = "Supplier/SupplierDropdownList?tenantId=" + tenantId;
                string json = await _getListApi.GetApiList(apiUrlSupplier);
                if (string.IsNullOrWhiteSpace(json)) return new List<SuppliersDropDown>();

                var parsed = JObject.Parse(json);
                return JsonConvert.DeserializeObject<List<SuppliersDropDown>>(
                    parsed["data"]?.ToString() ?? "[]"
                ) ?? new List<SuppliersDropDown>();
            }
            catch
            {
                return new List<SuppliersDropDown>();
            }
        }

        [HttpGet]
        private async Task<List<WarehousesDropDown>> GetWarehouseList(string tenantId)
        {
            try
            {
                string apiUrlWarehouse = "Warehouse/WarehouseDropdownList?tenantId=" + tenantId;
                string json = await _getListApi.GetApiList(apiUrlWarehouse);
                if (string.IsNullOrWhiteSpace(json)) return new List<WarehousesDropDown>();

                var parsed = JObject.Parse(json);
                return JsonConvert.DeserializeObject<List<WarehousesDropDown>>(
                    parsed["data"]?.ToString() ?? "[]"
                ) ?? new List<WarehousesDropDown>();
            }
            catch
            {
                return new List<WarehousesDropDown>();
            }
        }

        [HttpGet]
        private async Task<List<ProductModel>> GetProductList(string tenantId)
        {
            try
            {
                string apiUrlProduct = "Product/GetProducts";
                string json = await _getListApi.GetApiList(apiUrlProduct);
                if (string.IsNullOrWhiteSpace(json)) return new List<ProductModel>();

                var parsed = JObject.Parse(json);
                return JsonConvert.DeserializeObject<List<ProductModel>>(
                    parsed["data"]?.ToString() ?? "[]"
                ) ?? new List<ProductModel>();
            }
            catch
            {
                return new List<ProductModel>();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProjectInvoices(int projectId)
        {
            string apiUrl = "SalesInvoice/GetByProjectId?projectId=" + projectId;
            try
            {
                var json = await _getListApi.GetApiList(apiUrl);
                if (string.IsNullOrWhiteSpace(json))
                {
                    ViewBag.ErrorMessage = "پاسخی از سرور دریافت نشد.";
                    return View(new List<SalesInvoiceModel>());
                }

                var result = JsonConvert.DeserializeObject<ApiResponse<List<SalesInvoiceModel>>>(json);
                var model = result?.Data ?? new List<SalesInvoiceModel>();
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "خطا در دریافت فاکتورهای پروژه: " + ex.Message;
                return View(new List<SalesInvoiceModel>());
            }
        }
    }
}
