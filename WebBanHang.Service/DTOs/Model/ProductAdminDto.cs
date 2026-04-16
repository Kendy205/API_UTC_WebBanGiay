namespace WebBanHang.Service.DTOs.Model
{
    public class ProductAdminDto
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }

        public long CategoryId { get; set; }
        public long BrandId { get; set; }
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int Sold { get; set; }
        public IEnumerable<ProductVariantDto>? ProductVariants { get; set; } 
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
