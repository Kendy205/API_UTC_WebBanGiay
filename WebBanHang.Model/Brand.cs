using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Model
{
    [Table("brands")]
    public class Brand
    {
        [Key]
        [Column("brand_id")]
        public long BrandId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("brand_name")]
        public string BrandName { get; set; } = string.Empty;

        [MaxLength(180)]
        [Column("slug")]
        public string? Slug { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation Properties ──────────────────────────────
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
