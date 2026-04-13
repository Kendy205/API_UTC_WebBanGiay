namespace WebBanHang.Service.DTOs.Payment
{
    public class AdminPaymentQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Status { get; set; }
        public string? Method { get; set; }
    }
}
