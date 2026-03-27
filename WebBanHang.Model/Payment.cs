using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Model;

namespace WebBanHang.Model
{
    [Table("payments")]
    public class Payment
    {
        [Key]
        [Column("payment_id")]
        public long PaymentId { get; set; }

        [Column("order_id")]
        public long OrderId { get; set; }

        /// <summary>COD | bank_transfer | card | ewallet</summary>
        [MaxLength(30)]
        [Column("payment_method")]
        public string PaymentMethod { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("payment_provider")]
        public string? PaymentProvider { get; set; }

        [MaxLength(100)]
        [Column("transaction_code")]
        public string? TransactionCode { get; set; }

        [Column("amount", TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        /// <summary>pending | success | failed | refunded</summary>
        [MaxLength(20)]
        [Column("payment_status")]
        public string PaymentStatus { get; set; } = "pending";

        [Column("paid_at")]
        public DateTime? PaidAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation Properties ──────────────────────────────
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = null!;
    }
}

