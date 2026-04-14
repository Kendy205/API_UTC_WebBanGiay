namespace WebBanHang.Service.DTOs.Dashboard
{
    /// <summary>DTO cho phân bổ trạng thái đơn hàng</summary>
    public class AnalyticsOrderStatusDto
    {
        /// <summary>Tên trạng thái đơn hàng (pending, confirmed, packed, shipped, delivered, cancelled, refunded)</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Số lượng đơn hàng tương ứng với trạng thái đó</summary>
        public int Count { get; set; }

        /// <summary>Tỷ lệ phần trăm</summary>
        public decimal Percentage { get; set; }
    }
}
