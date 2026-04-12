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

        /// <summary>Lấy tổng quan đầu ngày (Header Dashboard)</summary>
        public async Task<DashboardHeaderSummaryDto> GetHeaderSummaryAsync()
        {
            //  Tính thời gian hôm nay

            var now = DateTime.UtcNow;
            var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var todayEnd = now; // Tới giờ hiện tại

            //  Lấy dữ liệu đơn hàng hôm nay

            var todayOrders = await _unitOfWork.Order.GetAllAsync(
                x => x.CreatedAt >= todayStart && x.CreatedAt <= todayEnd
            );

            var todayOrdersList = todayOrders.ToList();

            //  Tính toán doanh số hôm nay
    
            var todayRevenue = todayOrdersList.Sum(o => o.TotalAmount);
            var todayOrderCount = todayOrdersList.Count;

            //  Tính thời gian hôm qua

            var yesterdayStart = todayStart.AddDays(-1);
            var yesterdayEnd = todayStart.AddSeconds(-1);

            //  Lấy dữ liệu đơn hàng hôm qua

            var yesterdayOrders = await _unitOfWork.Order.GetAllAsync(
                x => x.CreatedAt >= yesterdayStart && x.CreatedAt <= yesterdayEnd
            );

            var yesterdayOrdersList = yesterdayOrders.ToList();
            var yesterdayRevenue = yesterdayOrdersList.Sum(o => o.TotalAmount);

            //  Tính toán so sánh doanh số hôm nay và hôm qua
            
            var difference = todayRevenue - yesterdayRevenue;
            var percentageChange = yesterdayRevenue > 0
                ? Math.Round((difference / yesterdayRevenue) * 100, 2)
                : 0;

            var revenueComparison = new ComparisonDto
            {
                Difference = difference,
                PercentageChange = percentageChange,
                IsGrowth = difference > 0
            };

            //  Lấy sản phẩm cạn hàng (stock < 50)

            var lowStockVariants = await _unitOfWork.ProductVariant.GetAllAsync(
                x => x.StockQuantity < 50 && x.IsActive,
                includeProperties: "Product,Size,Color"
            );

            var lowStockList = lowStockVariants
                .OrderBy(v => v.StockQuantity) // Sắp xếp theo số lượng (ít nhất trước)
                .Take(10) // Giới hạn 10 item
                .Select(v => new ProductLowStockDto
                {
                    ProductId = v.ProductId,
                    VariantId = v.VariantId,
                    ProductName = v.Product?.ProductName ?? string.Empty,
                    Sku = v.Sku,
                    SizeLabel = v.Size?.SizeLabel,
                    ColorName = v.Color?.ColorName,
                    StockQuantity = v.StockQuantity,
                    LowStockThreshold = v.LowStockThreshold
                })
                .ToList();

            //  Tính tổng sản phẩm cạn hàng

            var totalLowStockProducts = lowStockVariants.Count();

            //  Tạo response

            return new DashboardHeaderSummaryDto
            {
                TodayRevenue = todayRevenue,
                TodayOrderCount = todayOrderCount,
                RevenueComparison = revenueComparison,
                LowStockProducts = lowStockList,
                TotalLowStockProducts = totalLowStockProducts,
                FetchedAt = DateTime.UtcNow
            };
        }

        public async Task<AnalyticsOverviewDto> GetAnalyticsOverviewAsync()
        {
            var now = DateTime.UtcNow;
            var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endOfYear = new DateTime(now.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            // Get all orders for current year
            var allOrders = await _unitOfWork.Order.GetAllAsync(
                x => x.CreatedAt >= startOfYear && x.CreatedAt <= endOfYear
            );

            var orderList = allOrders.ToList();

            var totalRevenue = orderList.Sum(o => o.TotalAmount);
            var totalOrders = orderList.Count;
            var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            // Get new customers (users who made their first order in current year)
            var allUsers = await _unitOfWork.User.GetAllAsync();
            var userList = allUsers.ToList();

            var newCustomers = 0;
            foreach (var user in userList)
            {
                var firstOrder = orderList.Where(o => o.UserId == user.UserId).OrderBy(o => o.CreatedAt).FirstOrDefault();
                if (firstOrder != null && firstOrder.CreatedAt >= startOfYear && firstOrder.CreatedAt <= endOfYear)
                {
                    newCustomers++;
                }
            }

            // Revenue by month
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

            // Orders by status
            var ordersByStatus = new Dictionary<string, int>();
            var statusGroups = orderList.GroupBy(o => o.OrderStatus);
            foreach (var group in statusGroups)
            {
                ordersByStatus[group.Key] = group.Count();
            }

            return new AnalyticsOverviewDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                NewCustomers = newCustomers,
                AvgOrderValue = avgOrderValue,
                RevenueByMonth = revenueByMonth,
                OrdersByStatus = ordersByStatus
            };
        }

        public async Task<IEnumerable<RevenueChartDto>> GetRevenueChartAsync(DateTime from, DateTime to)
        {
            var orders = await _unitOfWork.Order.GetAllAsync(
                x => x.CreatedAt >= from && x.CreatedAt <= to
            );

            var orderList = orders.ToList();
            var revenueChart = new List<RevenueChartDto>();

            // Group by date
            var groupedByDate = orderList.GroupBy(o => o.CreatedAt.Date);

            foreach (var dateGroup in groupedByDate.OrderBy(g => g.Key))
            {
                var dateOrders = dateGroup.ToList();
                revenueChart.Add(new RevenueChartDto
                {
                    Date = dateGroup.Key.ToString("MM/dd"),
                    Revenue = dateOrders.Sum(o => o.TotalAmount),
                    Orders = dateOrders.Count
                });
            }

            return revenueChart;
        }

        public async Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to, string groupBy = "day")
        {
            var orders = await _unitOfWork.Order.GetAllAsync(
                x => x.CreatedAt >= from && x.CreatedAt <= to
            );

            var orderList = orders.ToList();

            var summary = new SalesReportSummaryDto
            {
                TotalRevenue = orderList.Sum(o => o.TotalAmount),
                TotalOrders = orderList.Count,
                AvgOrderValue = orderList.Count > 0 ? orderList.Sum(o => o.TotalAmount) / orderList.Count : 0,
                ReturnRate = 2.4m // This is a placeholder - you may need to calculate based on your business logic
            };

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
                foreach (var monthGroup in groupedByMonth.OrderBy(g => (g.Key.Year, g.Key.Month)))
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
