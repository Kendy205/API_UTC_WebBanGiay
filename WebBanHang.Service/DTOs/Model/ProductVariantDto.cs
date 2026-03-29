namespace WebBanHang.Service.DTOs.Model {
    public class ProductVariantDto {
        public long VariantId { get; set; }
        public long ProductId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public decimal? PriceOverride { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }

        public string? SizeName { get; set; }
        public string? ColorName { get; set; }
    }
}
