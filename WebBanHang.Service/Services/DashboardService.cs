using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.BLL.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Dashboard;

namespace WebBanHang.BLL.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<SalesStatisticDto>> GetSalesStatisticsAsync(string groupBy, int year)
        {
            //   Validate input
            var validGroupBy = new[] { "MONTH", "QUARTER", "YEAR" };
            if (!validGroupBy.Contains(groupBy?.ToUpper()))
                throw new ArgumentException("groupBy phải là MONTH, QUARTER hoặc YEAR");

            if (year < 2000 || year > DateTime.UtcNow.Year + 10)
                throw new ArgumentException("Năm không hợp lệ");

            //  Lấy toàn bộ đơn hàng trong năm
            var startOfYear = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfYear = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            var orders = await _unitOfWork.Order.GetAllAsync(
                x => x.CreatedAt >= startOfYear && x.CreatedAt <= endOfYear
            );

            // : Nhóm dữ liệu theo groupBy
            IEnumerable<SalesStatisticDto> result = groupBy?.ToUpper() switch
            {
                "MONTH" => GroupByMonth(orders, year),
                "QUARTER" => GroupByQuarter(orders, year),
                "YEAR" => GroupByYear(orders, year),
                _ => throw new ArgumentException("groupBy không hợp lệ")
            };

            return result;
        }

        /// Lấy thống kê tổng quan
        public async Task<SummaryStatisticDto> GetSummaryStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            //  Xác định khoảng thời gian
            var start = startDate ?? new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = endDate ?? DateTime.UtcNow;

            // Lấy dữ liệu từ database
            var orders = await _unitOfWork.Order.GetAllAsync(
                x => x.CreatedAt >= start && x.CreatedAt <= end,
                includeProperties: "OrderItems"
            );

            var orderList = orders.ToList();

            //  Tính toán các metric

            // Tổng số lượng sản phẩm bán được
            var totalProductsSold = orderList
                .SelectMany(o => o.OrderItems)
                .Sum(oi => oi.Quantity);

            // Số khách hàng độc nhất đã đặt hàng
            var totalUsersWithOrders = orderList
                .Select(o => o.UserId)
                .Distinct()
                .Count();

            // Tổng doanh thu
            var totalRevenue = orderList.Sum(o => o.TotalAmount);

            // Tổng số đơn hàng
            var totalOrders = orderList.Count;

            // Doanh thu trung bình
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            //  Trả về result
            return new SummaryStatisticDto
            {
                TotalProductsSold = totalProductsSold,
                TotalUsersWithOrders = totalUsersWithOrders,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue = averageOrderValue
            };
        }

        public async Task<IEnumerable<OrderStatusDto>> GetOrderStatusDistributionAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            //  Xác định khoảng thời gian

            var start = startDate ?? new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = endDate ?? DateTime.UtcNow;

            //  Lấy dữ liệu từ database

            var orders = await _unitOfWork.Order.GetAllAsync(
                x => x.CreatedAt >= start && x.CreatedAt <= end
            );

            var orderList = orders.ToList();

            //  Nhóm theo status và tính tỷ lệ

            var totalOrders = orderList.Count;
            if (totalOrders == 0)
                return new List<OrderStatusDto>();

            var statusDistribution = orderList
                .GroupBy(o => o.OrderStatus)
                .Select(g => new OrderStatusDto
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Percentage = Math.Round((decimal)g.Count() / totalOrders * 100, 2)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return statusDistribution;
        }

        /// Nhóm doanh số theo tháng
        private List<SalesStatisticDto> GroupByMonth(IEnumerable<Order> orders, int year)
        {
            var monthNames = new[]
            {
                "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6",
                "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"
            };

            var result = new List<SalesStatisticDto>();

            for (int month = 1; month <= 12; month++)
            {
                var monthOrders = orders.Where(o =>
                    o.CreatedAt.Year == year && o.CreatedAt.Month == month
                ).ToList();

                result.Add(new SalesStatisticDto
                {
                    TimeLabel = monthNames[month - 1],
                    TotalRevenue = monthOrders.Sum(o => o.TotalAmount),
                    TotalOrders = monthOrders.Count
                });
            }

            return result;
        }

        /// Nhóm doanh số theo quý
        private List<SalesStatisticDto> GroupByQuarter(IEnumerable<Order> orders, int year)
        {
            var result = new List<SalesStatisticDto>();

            for (int quarter = 1; quarter <= 4; quarter++)
            {
                var startMonth = (quarter - 1) * 3 + 1;
                var endMonth = quarter * 3;

                var quarterOrders = orders.Where(o =>
                    o.CreatedAt.Year == year &&
                    o.CreatedAt.Month >= startMonth &&
                    o.CreatedAt.Month <= endMonth
                ).ToList();

                result.Add(new SalesStatisticDto
                {
                    TimeLabel = $"Quý {quarter}",
                    TotalRevenue = quarterOrders.Sum(o => o.TotalAmount),
                    TotalOrders = quarterOrders.Count
                });
            }

            return result;
        }

        /// Nhóm doanh số theo năm
        private List<SalesStatisticDto> GroupByYear(IEnumerable<Order> orders, int year)
        {
            var yearOrders = orders.Where(o => o.CreatedAt.Year == year).ToList();

            return new List<SalesStatisticDto>
            {
                new SalesStatisticDto
                {
                    TimeLabel = year.ToString(),
                    TotalRevenue = yearOrders.Sum(o => o.TotalAmount),
                    TotalOrders = yearOrders.Count
                }
            };
        }
    }
}
