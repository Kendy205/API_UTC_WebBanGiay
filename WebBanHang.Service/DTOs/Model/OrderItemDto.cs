namespace WebBanHang.Service.DTOs.Model {
    public class OrderItemDto {
        public long OrderItemId { get; set; }
        public long OrderId { get; set; }
        public long VariantId { get; set; }
        public long ProductId { get; set; }
        public string ProductNameSnapshot { get; set; } = string.Empty;
        public string? SizeLabelSnapshot { get; set; }
        public string? ColorNameSnapshot { get; set; }
        public string? SkuSnapshot { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
