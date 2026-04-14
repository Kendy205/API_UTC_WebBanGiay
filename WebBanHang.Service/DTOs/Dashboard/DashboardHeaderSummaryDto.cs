namespace WebBanHang.Service.DTOs.Dashboard
{
    /// <summary>DTO cho tổng quan đầu ngày (Header Dashboard)</summary>
    public class DashboardHeaderSummaryDto
    {
        /// <summary>Doanh thu ngày hôm nay</summary>
        public decimal TodayRevenue { get; set; }

        /// <summary>Tổng số đơn hàng mới phát sinh trong ngày hôm nay</summary>
        public int TodayOrderCount { get; set; }

        /// <summary>So sánh doanh thu với hôm qua</summary>
        public ComparisonDto RevenueComparison { get; set; } = new();

        /// <summary>Danh sách sản phẩm cạn hàng/hết hàng (stock < 50) - tối đa 10 item</summary>
        public List<ProductLowStockDto> LowStockProducts { get; set; } = new();

        /// <summary>Tổng số sản phẩm đang cảnh báo hết hàng</summary>
        public int TotalLowStockProducts { get; set; }

        /// <summary>Thời gian lấy dữ liệu</summary>
        public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    }
}
