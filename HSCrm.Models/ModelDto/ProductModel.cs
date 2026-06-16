namespace HSCrm.Models.ModelDto
{
    public class ProductModel
    {
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public required string ProductUnit { get; set; }
        public bool IsActive { get; set; }
        public decimal? SellPrice { get; set; }
        public decimal? BuyPrice { get; set; }
        public required int TenantId { get; set; }
    }
}
