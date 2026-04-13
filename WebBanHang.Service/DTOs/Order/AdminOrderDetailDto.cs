using System.Collections.Generic;

namespace WebBanHang.Service.DTOs.Order
{
    public class AdminOrderDetailItemDto
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class AdminOrderDetailDto
    {
        public long Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public List<AdminOrderDetailItemDto> Items { get; set; } = new();
    }
}
