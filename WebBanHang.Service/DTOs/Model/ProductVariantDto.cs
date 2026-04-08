namespace WebBanHang.Service.DTOs.Model {
    public class ProductVariantDto {
        public long VariantId { get; set; }
        public long ProductId { get; set; }
        //public int SizeId { get; set; }
        //public int ColorId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public decimal? PriceOverride { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public string? SizeLabel { get; set; }
        public string? SizeSystem { get; set; }
        public string? ColorName { get; set; }
        public string? ProductName { get; set; }

    }
}
