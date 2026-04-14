namespace WebBanHang.Service.DTOs.Dashboard
{
    /// <summary>DTO cho thống kê tổng quan</summary>
    public class SummaryStatisticDto
    {
        /// <summary>Tổng số lượng sản phẩm đã bán ra</summary>
        public int TotalProductsSold { get; set; }

        /// <summary>Số lượng khách hàng thực tế (người dùng đã từng có ít nhất 1 đơn hàng)</summary>
        public int TotalUsersWithOrders { get; set; }

        /// <summary>Tổng doanh thu tất cả thời gian</summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>Tổng số đơn hàng tất cả thời gian</summary>
        public int TotalOrders { get; set; }

        /// <summary>Doanh thu trung bình mỗi đơn hàng</summary>
        public decimal AverageOrderValue { get; set; }
    }
}
