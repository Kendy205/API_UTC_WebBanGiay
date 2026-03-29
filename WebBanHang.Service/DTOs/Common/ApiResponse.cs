using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;

namespace WebBanHang.DTOs.Common
{
    public class ApiResponse<T>
    {
        /// <summary>Trạng thái: true (thành công) / false (thất bại)</summary>
        public bool Success { get; set; }

        /// <summary>Mã lỗi HTTP (200, 400, 401, 404, 500...)</summary>
        public int StatusCode { get; set; }

        /// <summary>Thông báo cho người dùng (VD: "Đăng nhập thành công", "Không tìm thấy sản phẩm")</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Dữ liệu chính trả về (Danh sách sản phẩm, thông tin user...)</summary>
        public T? Data { get; set; }

        /// <summary>Chi tiết lỗi (nếu có, thường dùng cho lỗi Validate form)</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Nếu không có lỗi thì ẩn trường này đi cho gọn
        public object? Errors { get; set; }

        // ── CÁC HÀM HỖ TRỢ TẠO NHANH (FACTORY METHODS) ──────────────────────

        // Dùng khi gọi API Thành công
        public static ApiResponse<T> Succeeded(T data, string message = "Thành công", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        // Dùng khi gọi API Thất bại
        public static ApiResponse<T> Failed(string message, int statusCode = 400, object? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Data = default, // null
                Errors = errors
            };
        }
    }
}
