using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Service.DTOs.Model {
    public class RoleDto {
        // TODO: Thêm các property cần thiết trả về cho API
        public short RoleId { get; set; }

      
        public string RoleName { get; set; } = string.Empty;

    
        public string? Description { get; set; }
    }
}
