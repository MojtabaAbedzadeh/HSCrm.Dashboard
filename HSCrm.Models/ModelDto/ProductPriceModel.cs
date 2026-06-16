namespace HSCrm.Models.ModelDto
{
    public class ProductPriceModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public ProductModel Product { get; set; } = default!;
        public required decimal SellPrice { get; set; }
        public decimal BuyPrice { get; set; }
        public bool IsActive { get; set; }
        public string EffectiveFrom { get; set; }
        public string EffectiveTo { get; set; }
    }
}
