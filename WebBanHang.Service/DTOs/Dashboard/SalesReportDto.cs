namespace WebBanHang.Service.DTOs.Dashboard
{
    public class SalesReportDto
    {
        public SalesReportSummaryDto Summary { get; set; } = new();
        public List<SalesReportPeriodDto> Data { get; set; } = new();
    }

    public class SalesReportSummaryDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AvgOrderValue { get; set; }
        public decimal ReturnRate { get; set; }
    }

    public class SalesReportPeriodDto
    {
        public string Period { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public decimal AvgValue { get; set; }
    }
}
