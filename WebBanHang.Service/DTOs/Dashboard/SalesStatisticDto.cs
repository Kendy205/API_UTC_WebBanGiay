namespace WebBanHang.Service.DTOs.Dashboard
{
    /// <summary>DTO cho thống kê doanh số theo thời gian</summary>
    public class SalesStatisticDto
    {
        /// <summary>Nhãn thời gian (VD: "Tháng 1", "Quý 2", "2024")</summary>
        public string TimeLabel { get; set; } = string.Empty;

        /// <summary>Tổng doanh thu</summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>Tổng số đơn hàng</summary>
        public int TotalOrders { get; set; }
    }
}
