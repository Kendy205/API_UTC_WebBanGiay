using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.Service.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Dashboard;

namespace WebBanHang.Service.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

    

        /// Lấy thống kê tổng quan
        public async Task<DashboardSummaryStatisticDto> GetSummaryStatisticsAsync()
        {
            // 1. Xác định khoảng thời gian là tháng hiện tại (UTC)
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1).AddSeconds(-1); // 23:59:59 ngày cuối tháng

            // 2. Lấy dữ liệu đơn hàng trong khoảng
            var orders = await _unitOfWork.Order.GetAllAsync(
                x => x.CreatedAt >= start && x.CreatedAt <= end,
                includeProperties: "OrderItems"
            );
            var orderList = orders.ToList();

            // 3. Tính toán các chỉ số
            var totalRevenue = orderList.Sum(o => o.TotalAmount);
            var totalOrders = orderList.Count;
            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            // Giả sử Order có property bool IsReturned (hoặc OrderStatus)
            var returnedOrders = orderList.Count(o => o.OrderStatus.Equals("Cancelled"));
            var returnRate = totalOrders > 0 ? (decimal)returnedOrders / totalOrders * 100 : 0;

            return new DashboardSummaryStatisticDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AvgOrderValue = avgOrderValue,
                ReturnRate = returnRate
            };
        }

        public async Task<IEnumerable<AnalyticsOrderStatusDto>> GetOrderStatusDistributionAsync(int year)
        {
            // Xác định khoảng thời gian từ đầu năm đến cuối năm
            var start = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            // Lấy đơn hàng trong năm
            var orders = await _unitOfWork.Order.GetAllAsync(
                x => x.CreatedAt >= start && x.CreatedAt <= end
            );
            var orderList = orders.ToList();

            var totalOrders = orderList.Count;
            if (totalOrders == 0)
                return new List<AnalyticsOrderStatusDto>();

            // Nhóm theo trạng thái và tính tỷ lệ phần trăm
            var statusDistribution = orderList
                .GroupBy(o => o.OrderStatus)
                .Select(g => new AnalyticsOrderStatusDto
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Percentage = Math.Round((decimal)g.Count() / totalOrders * 100, 2)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return statusDistribution;
        }

        


        public async Task<AnalyticsOverviewDto> GetAnalyticsOverviewAsync(int year)
        {
            // Xác định đầu và cuối năm theo yêu cầu (UTC)
            var startOfYear = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfYear = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            // Lấy tất cả đơn hàng trong năm được chọn
            var allOrders = await _unitOfWork.Order.GetAllAsync(
                x => x.CreatedAt >= startOfYear && x.CreatedAt <= endOfYear
            );
            var orderList = allOrders.ToList();

            // Tính tổng doanh thu, tổng đơn, giá trị đơn hàng TB
            var totalRevenue = orderList.Sum(o => o.TotalAmount);
            var totalOrders = orderList.Count;
            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            // Tính khách hàng mới (lần đầu đặt hàng trong năm này)
            var allUsers = await _unitOfWork.User.GetAllAsync();
            var userList = allUsers.ToList();

            int newCustomers = 0;
            foreach (var user in userList)
            {
                var firstOrder = orderList
                    .Where(o => o.UserId == user.UserId)
                    .OrderBy(o => o.CreatedAt)
                    .FirstOrDefault();

                if (firstOrder != null && firstOrder.CreatedAt >= startOfYear && firstOrder.CreatedAt <= endOfYear)
                {
                    newCustomers++;
                }
            }

            // Doanh thu theo tháng (T1, T2, ..., T12)
            var revenueByMonth = new List<RevenueByMonthDto>();
            var monthNames = new[] { "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12" };

            for (int month = 1; month <= 12; month++)
            {
                var monthOrders = orderList.Where(o => o.CreatedAt.Month == month).ToList();
                revenueByMonth.Add(new RevenueByMonthDto
                {
                    Month = monthNames[month - 1],
                    Revenue = monthOrders.Sum(o => o.TotalAmount)
                });
            }

            // Trả về DTO (không còn OrdersByStatus)
            return new AnalyticsOverviewDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                NewCustomers = newCustomers,
                AvgOrderValue = avgOrderValue,
                RevenueByMonth = revenueByMonth
            };
        }



        public async Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to, string groupBy = "day")
        {
            var orders = await _unitOfWork.Order.GetAllAsync(
                x => x.CreatedAt >= from && x.CreatedAt <= to
            );
            var orderList = orders.ToList();

            // Tính tỷ lệ đơn hàng bị huỷ (Cancelled)
            int cancelledOrders = 0;
            if (orderList.Any())
            {
                // Giả sử OrderStatus là enum hoặc string, điều chỉnh theo thực tế
                cancelledOrders = orderList.Count(o => o.OrderStatus.Equals("Cancelled"));
                // Nếu OrderStatus là string: o.OrderStatus == "Cancelled"
            }
            decimal returnRate = orderList.Count > 0 ? (decimal)cancelledOrders / orderList.Count * 100 : 0;

            var summary = new SalesReportSummaryDto
            {
                TotalRevenue = orderList.Sum(o => o.TotalAmount),
                TotalOrders = orderList.Count,
                AvgOrderValue = orderList.Count > 0 ? orderList.Sum(o => o.TotalAmount) / orderList.Count : 0,
                ReturnRate = Math.Round(returnRate, 2)   // Làm tròn 2 chữ số thập phân
            };

            // Phần còn lại giữ nguyên: tạo data theo day/week/month
            var data = new List<SalesReportPeriodDto>();

            if (groupBy?.ToLower() == "day")
            {
                var groupedByDate = orderList.GroupBy(o => o.CreatedAt.Date);
                foreach (var dateGroup in groupedByDate.OrderBy(g => g.Key))
                {
                    var dateOrders = dateGroup.ToList();
                    var revenue = dateOrders.Sum(o => o.TotalAmount);
                    var count = dateOrders.Count;

                    data.Add(new SalesReportPeriodDto
                    {
                        Period = dateGroup.Key.ToString("dd/MM/yyyy"),
                        Revenue = revenue,
                        Orders = count,
                        AvgValue = count > 0 ? revenue / count : 0
                    });
                }
            }
            else if (groupBy?.ToLower() == "week")
            {
                var groupedByWeek = orderList.GroupBy(o =>
                {
                    var date = o.CreatedAt.Date;
                    int dayOfWeek = (int)date.DayOfWeek;
                    var startOfWeek = date.AddDays(-dayOfWeek);
                    return startOfWeek;
                });

                foreach (var weekGroup in groupedByWeek.OrderBy(g => g.Key))
                {
                    var weekOrders = weekGroup.ToList();
                    var revenue = weekOrders.Sum(o => o.TotalAmount);
                    var count = weekOrders.Count;
                    var endOfWeek = weekGroup.Key.AddDays(6);

                    data.Add(new SalesReportPeriodDto
                    {
                        Period = $"({weekGroup.Key:dd/MM} - {endOfWeek:dd/MM})",
                        Revenue = revenue,
                        Orders = count,
                        AvgValue = count > 0 ? revenue / count : 0
                    });
                }
            }
            else if (groupBy?.ToLower() == "month")
            {
                var groupedByMonth = orderList.GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month });
                foreach (var monthGroup in groupedByMonth.OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month))
                {
                    var monthOrders = monthGroup.ToList();
                    var revenue = monthOrders.Sum(o => o.TotalAmount);
                    var count = monthOrders.Count;

                    data.Add(new SalesReportPeriodDto
                    {
                        Period = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1).ToString("MMMM yyyy"),
                        Revenue = revenue,
                        Orders = count,
                        AvgValue = count > 0 ? revenue / count : 0
                    });
                }
            }

            return new SalesReportDto
            {
                Summary = summary,
                Data = data
            };
        }
    }
}
