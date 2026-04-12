namespace WebBanHang.Service.DTOs.Dashboard
{
    /// <summary>DTO để so sánh dữ liệu giữa hai kỳ</summary>
    public class ComparisonDto
    {
        /// <summary>Chênh lệch giá trị (Today - Yesterday)</summary>
        public decimal Difference { get; set; }

        /// <summary>Phần trăm thay đổi ((Today - Yesterday) / Yesterday * 100)</summary>
        public decimal PercentageChange { get; set; }

        /// <summary>True nếu tăng, False nếu giảm</summary>
        public bool IsGrowth { get; set; }

        /// <summary>Trạng thái so sánh (tăng/giảm/không thay đổi)</summary>
        public string Status => IsGrowth ? "Tăng" : PercentageChange == 0 ? "Không thay đổi" : "Giảm";
    }
}
