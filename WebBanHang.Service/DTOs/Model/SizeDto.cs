namespace WebBanHang.Service.DTOs.Model {
    public class SizeDto {
        // TODO: Thêm các property cần thiết trả về cho API
        public long SizeId { get; set; }
        public string SizeLabel { get; set; } = string.Empty;
        public string? SizeSystem { get; set; }
        public int SortOrder { get; set; }

    }
}
