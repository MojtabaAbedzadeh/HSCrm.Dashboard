using HSCrm.Models.ModelDto;
using static HSCrm.Common.PublicTools.Enums;

namespace HSCrm.Models.ModelDto
{
    public class PurchaseInvoiceModel
    {
        public int Id { get; set; }
        public required string Number { get; set; }
        public int SupplierId { get; set; }
        public SupplierModel Supplier { get; set; } = default!;
        public int WarehouseId { get; set; }
        public WarehousesModel Warehouses { get; set; }

        public DateTime IssueDate { get; set; }
        public byte PayStatus { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string? Notes { get; set; }
        public ICollection<PurchaseInvoiceItemModel> Items { get; set; }
            = new List<PurchaseInvoiceItemModel>();
    }
    public class PurInvoiceModel
    {
        public int Id { get; set; }
        public required string Number { get; set; }

        public int WarehouseId { get; set; }
        public WarehousesModel Warehouse { get; set; } = default!;

        public string PurchaserUserId { get; set; }
        public UserModel User { get; set; }

        public int SupplierId { get; set; }
        public SupplierModel Supplier { get; set; } = default!;

        public SalesInvoiceStatus Status { get; set; }

        public DateTime IssueDate { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public SettlementStatus PayStatus { get; set; } = SettlementStatus.Unpaid;
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount => Total - PaidAmount;


        public ICollection<PurchaseInvoiceItemModel> Items { get; set; } = new List<PurchaseInvoiceItemModel>();
        public int FiscalYearId { get; set; }
        public FiscalYearDto FiscalYear { get; set; } = default!;
    }
    public class EditPurchaseInvoiceModel
    {
        public int Id { get; set; }
        public required string Number { get; set; }

        // انبار
        public int WarehouseId { get; set; }
        public WarehousesModel? Warehouse { get; set; }

        // تأمین‌کننده (پروژه حذف شد)
        public int SupplierId { get; set; }
        public SupplierModel? Supplier { get; set; }

        public DateTime IssueDate { get; set; }
        public byte PayStatus { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        // مبلغ باقی‌مانده (محاسباتی)
        public decimal RemainingAmount => Total - PaidAmount;

        public string? Notes { get; set; }

        // لیست اقلام فاکتور
        public ICollection<PurchaseInvoiceItemModel> Items { get; set; }  = new List<PurchaseInvoiceItemModel>();
    }
    public class PurchaseInvoiceEditModel
    {
        public int Id { get; set; }
        public required string Number { get; set; }

        // انبار
        public int WarehouseId { get; set; }
        public WarehousesModel? Warehouse { get; set; }
        public SettlementStatus PayStatus { get; set; } = SettlementStatus.Unpaid;
        public decimal RemainingAmount { get; set; }


        public string PurchaserUserId { get; set; }
        public SalesInvoiceStatus Status { get; set; }


        // تأمین‌کننده
        public int SupplierId { get; set; }
        public SupplierModel? Supplier { get; set; }

        public DateTime IssueDate { get; set; }

        public byte PaymentStatus { get; set; }
        public decimal PaidAmount { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        public string? Notes { get; set; }

        // اقلام فاکتور
        public ICollection<PurchaseInvoiceItemModel> Items { get; set; }
            = new List<PurchaseInvoiceItemModel>();
    }
}

