using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Service.DTOs.Model {
    public class PaymentDto {
        // TODO: Thêm các property cần thiết trả về cho API
        public long PaymentId { get; set; }
        public long OrderId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = "pending";
        public string? TransactionCode { get; set; }
        public OrderDto Order { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
