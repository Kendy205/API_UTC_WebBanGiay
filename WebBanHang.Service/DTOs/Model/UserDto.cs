using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebBanHang.Model;

namespace WebBanHang.Service.DTOs.Model {
    public class UserDto {
        public long UserId { get; set; }
        public string? FullName { get; set; } = string.Empty;
        public string? Status { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; }

        // Bạn có thể "Làm phẳng" dữ liệu để Front-end dễ dùng
        // Ví dụ: Kéo tên Role từ bảng UserRole -> Role sang đây luôn
        public ICollection<UserRoleDto>? UserRoles { get; set; } = new List<UserRoleDto>();

        public string? RoleNames { get; set; } = string.Empty; // Ví dụ: "Admin, User"
    }
    public class UserResgiterDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Password { get; set; } = string.Empty;
        public string? Status { get; set; } = string.Empty;
        public short? RoleId { get; set; } = 2; // Ví dụ: "Admin, User"
    }
}
