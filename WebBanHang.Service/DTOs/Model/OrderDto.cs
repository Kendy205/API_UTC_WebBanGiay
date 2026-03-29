using System;
using System.Collections.Generic;

namespace WebBanHang.Service.DTOs.Model {
    public class OrderDto {
        public long OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string? CustomerName { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }
}
