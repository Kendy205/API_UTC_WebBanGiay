using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBanHang.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty; // Đổi tên cho rõ ràng
        public string RefreshToken { get; set; } = string.Empty; // Thêm dòng này
        public string Message { get; set; } = string.Empty;
    }
}
