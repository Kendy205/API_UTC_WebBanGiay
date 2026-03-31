namespace WebBanHang.Service.DTOs.Model {
    public class ProductDto {
        public long ProductId { get; set; }
        //public long CategoryId { get; set; }
        //public long BrandId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public string? Image { get; set; }
        // Flatten (Làm phẳng) dữ liệu từ các bảng liên kết
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }

    }
}
