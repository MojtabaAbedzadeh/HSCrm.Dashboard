namespace HSCrm.Models.ModelDto
{
    public class PaymentModel
    {
        public long Id { get; set; }
        public long FiscalYearId { get; set; }
        public int? SalesInvoiceId { get; set; }
        public int? PurchaseInvoiceId { get; set; }
        public int? ExpenseId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = "Cash";
        public string? ReferenceNo { get; set; }
    }
}
