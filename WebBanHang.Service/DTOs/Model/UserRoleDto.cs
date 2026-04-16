using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Service.DTOs.Model {
    public class UserRoleDto {
        // TODO: Thêm các property cần thiết trả về cho API
        public long UserId { get; set; }
        public short RoleId { get; set; }

    }
}
