namespace HSCrm.Models.ModelDto
{
    public class PurchaseInvoiceItemModel
    {
        public int ProductId { get; set; }
        public string ProductTitle { get; set; } // اضافه کردن فیلد برای نگهداری مستقیم نام کالا
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; } // طبق نام‌گذاری HSCrm
        public decimal UnitPricePurchase { get; set; } // برای تطابق کامل
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }

        // یا اگر کلاس محصول را داری:
        public ProductModel Product { get; set; }
    }

}