namespace WebBanHang.Service.DTOs.Model {
    public class UserDto {
        // TODO: Thêm các property cần thiết trả về cho API
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }

        // Bạn có thể "Làm phẳng" dữ liệu để Front-end dễ dùng
        // Ví dụ: Kéo tên Role từ bảng UserRole -> Role sang đây luôn
        public string? RoleName { get; set; }
    }
}
