namespace WebBanHang.Service.DTOs.Order
{
    public class AdminUpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class AdminOrderStatusResultDto
    {
        public long Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
