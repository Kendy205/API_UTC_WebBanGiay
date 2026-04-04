using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Model
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public long OrderId { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Column("shipping_address_id")]
        public long ShippingAddressId { get; set; }

        [MaxLength(50)]
        [Column("order_code")]
        public string OrderCode { get; set; } = string.Empty;

        /// <summary>pending | confirmed | packed | shipped | delivered | cancelled | refunded</summary>
        [MaxLength(30)]
        [Column("order_status")]
        public string OrderStatus { get; set; } = "pending";

        [Column("subtotal_amount", TypeName = "decimal(12,2)")]
        public decimal SubtotalAmount { get; set; }

        [Column("shipping_fee", TypeName = "decimal(12,2)")]
        public decimal ShippingFee { get; set; } = 0;

        [Column("discount_amount", TypeName = "decimal(12,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column("total_amount", TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>unpaid | paid | failed | refunded</summary>
        [MaxLength(20)]
        [Column("payment_status")]
        public string PaymentStatus { get; set; } = "unpaid";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation Properties ──────────────────────────────
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(ShippingAddressId))]
        public Address ShippingAddress { get; set; } = null!;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
