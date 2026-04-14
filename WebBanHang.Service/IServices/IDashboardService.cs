using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Dashboard;

namespace WebBanHang.BLL.IServices
{
    public interface IDashboardService
    {

        Task<IEnumerable<SalesStatisticDto>> GetSalesStatisticsAsync(string groupBy, int year);

        Task<SummaryStatisticDto> GetSummaryStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        Task<IEnumerable<OrderStatusDto>> GetOrderStatusDistributionAsync(DateTime? startDate = null, DateTime? endDate = null);


        Task<DashboardHeaderSummaryDto> GetHeaderSummaryAsync();

        // Analytics endpoints
        Task<AnalyticsOverviewDto> GetAnalyticsOverviewAsync();

        Task<IEnumerable<RevenueChartDto>> GetRevenueChartAsync(DateTime from, DateTime to);

        Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to, string groupBy = "day");
    }
}
