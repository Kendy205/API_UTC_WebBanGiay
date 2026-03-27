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
    [Table("reviews")]
    public class Review
    {
        [Key]
        [Column("review_id")]
        public long ReviewId { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Column("order_item_id")]
        public long OrderItemId { get; set; }

        [Range(1, 5)]
        [Column("rating")]
        public short Rating { get; set; }

        [MaxLength(150)]
        [Column("review_title")]
        public string? ReviewTitle { get; set; }

        [Column("review_content", TypeName = "text")]
        public string? ReviewContent { get; set; }

        [Column("is_public")]
        public bool IsPublic { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation Properties ──────────────────────────────
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(OrderItemId))]
        public OrderItem OrderItem { get; set; } = null!;
    }
}