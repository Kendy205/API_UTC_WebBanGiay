
﻿namespace WebBanHang.Service.DTOs.Model
{
    public class CartDto
    {
        public long CartId { get; set; }
        public long UserId { get; set; }
        public string Status { get; set; } = "Active"; // Active, Abandoned, Converted
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ── Items trong giỏ hàng ──────────────────────────
        public ICollection<CartItemDto> Items { get; set; } = new List<CartItemDto>();

        // ── Tính toán ──────────────────────────
        public int TotalItems => Items.Sum(x => x.Quantity);
        public decimal TotalPrice => Items.Sum(x => x.TotalPrice);

    }
}
