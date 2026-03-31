namespace WebBanHang.Service.DTOs.Model {
    public class ColorDto {
        // TODO: Thêm các property cần thiết trả về cho API
        public long ColorId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string? HexCode { get; set; }

    }
}
