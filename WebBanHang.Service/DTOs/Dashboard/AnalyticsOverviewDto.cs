namespace WebBanHang.Service.DTOs.Dashboard
{
    public class AnalyticsOverviewDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int NewCustomers { get; set; }
        public decimal AvgOrderValue { get; set; }
        public List<RevenueByMonthDto> RevenueByMonth { get; set; } = new();
    }

    public class RevenueByMonthDto
    {
        public string Month { get; set; }
        public decimal Revenue { get; set; }
    }
}
