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
    [Table("carts")]
    public class Cart
    {
        [Key]
        [Column("cart_id")]
        public long CartId { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        /// <summary>active | converted | abandoned</summary>
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "active";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation Properties ──────────────────────────────
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}

