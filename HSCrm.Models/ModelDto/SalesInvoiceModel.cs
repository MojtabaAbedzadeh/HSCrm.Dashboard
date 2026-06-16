namespace HSCrm.Models.ModelDto
{
    public class SalesInvoiceModel
    {
        public int Id { get; set; }
        public required string Number { get; set; }       
        public int? ProjectId { get; set; }
        public ProjectModel? Project { get; set; }
        public DateTime IssueDate { get; set; }
        public byte Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string? Notes { get; set; }
    }
}
