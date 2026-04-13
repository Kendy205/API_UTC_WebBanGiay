using System;
using System.Collections.Generic;

namespace WebBanHang.Service.DTOs.Order
{
    public class AdminOrderListItemDto
    {
        public long Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class AdminOrderListResponseDto
    {
        public List<AdminOrderListItemDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
