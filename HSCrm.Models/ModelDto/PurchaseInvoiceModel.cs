namespace HSCrm.Models.ModelDto
{
    public class PurchaseInvoiceModel
    {
        public int Id { get; set; }
        public required string Number { get; set; }
        public int SupplierId { get; set; }
        public SupplierModel Supplier { get; set; } = default!;
        public int? ProjectId { get; set; }
        public ProjectModel? Project { get; set; }
        public DateTime IssueDate { get; set; }
        public byte PayStatus { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string? Notes { get; set; }
    }
}
