using System;
using System.Collections.Generic;

namespace WebBanHang.Service.DTOs.Payment
{
    public class AdminPaymentListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public long OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AdminPaymentListResponseDto
    {
        public List<AdminPaymentListItemDto> Items { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
