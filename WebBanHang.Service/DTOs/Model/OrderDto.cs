using System;
using System.Collections.Generic;

namespace WebBanHang.Service.DTOs.Model {
    public class OrderDto {
        public long OrderId { get; set; }
        public long UserId { get; set; }
        public long ShippingAddressId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public decimal SubtotalAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string? CustomerName { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }
}
