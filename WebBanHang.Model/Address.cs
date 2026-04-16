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
    [Table("addresses")]
    public class Address
    {
        [Key]
        [Column("address_id")]
        public long AddressId { get; set; }

        [Column("user_id")]
        public long UserId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("recipient_name")]
        public string RecipientName { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("phone")]
        public string? Phone { get; set; }

        [MaxLength(100)]
        [Column("province")]
        public string? Province { get; set; }

        [MaxLength(100)]
        [Column("district")]
        public string? District { get; set; }

        [MaxLength(100)]
        [Column("ward")]
        public string? Ward { get; set; }

        [MaxLength(255)]
        [Column("street_address")]
        public string? StreetAddress { get; set; }

        [Column("is_default")]
        public bool IsDefault { get; set; } = false;
        [Column("is_deleted")]
        public bool IsDelete { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation Properties ──────────────────────────────
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

