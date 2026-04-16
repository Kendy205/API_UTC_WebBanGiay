using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Dashboard;

namespace WebBanHang.Service.IServices
{
    public interface IDashboardService
    {

        Task<DashboardSummaryStatisticDto> GetSummaryStatisticsAsync();

        Task<IEnumerable<AnalyticsOrderStatusDto>> GetOrderStatusDistributionAsync(int year);



        // Analytics endpoints
        Task<AnalyticsOverviewDto> GetAnalyticsOverviewAsync(int year);

        Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to, string groupBy = "day");
    }
}
