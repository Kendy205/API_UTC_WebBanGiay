using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Service.DTOs.Model {
    public class ReviewDto {
        // TODO: Thêm các property cần thiết trả về cho API
        public long ReviewId { get; set; }

        public long UserId { get; set; }

        public long OrderItemId { get; set; }

        public short Rating { get; set; }

        public string? ReviewContent { get; set; }
        public string? ReviewTitle { get; set; }



    }
}
