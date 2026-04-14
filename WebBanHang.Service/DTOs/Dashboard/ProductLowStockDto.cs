namespace WebBanHang.Service.DTOs.Dashboard
{
    /// <summary>DTO cho sản phẩm hết/cạn hàng</summary>
    public class ProductLowStockDto
    {
        /// <summary>ID sản phẩm</summary>
        public long ProductId { get; set; }

        /// <summary>ID biến thể</summary>
        public long VariantId { get; set; }

        /// <summary>Tên sản phẩm</summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>SKU</summary>
        public string Sku { get; set; } = string.Empty;

        /// <summary>Size</summary>
        public string? SizeLabel { get; set; }

        /// <summary>Màu</summary>
        public string? ColorName { get; set; }

        /// <summary>Số lượng tồn kho hiện tại</summary>
        public int StockQuantity { get; set; }

        /// <summary>Ngưỡng cảnh báo hết hàng</summary>
        public int LowStockThreshold { get; set; }
    }
}
