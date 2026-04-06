
﻿namespace WebBanHang.Service.DTOs.Model {
    public class CartItemDto
    {
        public long CartItemId { get; set; } 
        public long CartId { get; set; } 
        public long ProductId { get; set; }
        public long VariantId { get; set; } 
        public int Quantity { get; set; } 
        public int StockQuantity { get; set; }
        public decimal UnitPrice { get; set; } 
        public DateTime CreatedAt { get; set; } 

        // ── Flatten data từ ProductVariant ──────────────────────────
        public string? VariantSku { get; set; } 
        public string? SizeName { get; set; } 
        public string? ColorName { get; set; } 
        public string? ProductName { get; set; } 

        // ── Tính toán ──────────────────────────
        public decimal TotalPrice => UnitPrice * Quantity;


    }
}
