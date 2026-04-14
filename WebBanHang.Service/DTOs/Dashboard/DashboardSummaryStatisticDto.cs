namespace WebBanHang.Service.DTOs.Dashboard
{
    /// <summary>DTO cho thống kê tổng quan</summary>
    public class DashboardSummaryStatisticDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AvgOrderValue { get; set; }
        public decimal ReturnRate { get; set; }
    }
}
