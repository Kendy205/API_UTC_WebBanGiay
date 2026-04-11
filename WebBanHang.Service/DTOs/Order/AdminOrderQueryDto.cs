namespace WebBanHang.Service.DTOs.Order
{
    public class AdminOrderQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Status { get; set; }
        public string? Search { get; set; }
    }
}
