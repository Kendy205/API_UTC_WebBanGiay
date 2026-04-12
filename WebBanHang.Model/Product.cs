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
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("product_id")]
        public long ProductId { get; set; }

        [Column("category_id")]
        public long CategoryId { get; set; }

        [Column("brand_id")]
        public long BrandId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("product_name")]
        public string ProductName { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("slug")]
        public string? Slug { get; set; }

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Column("base_price", TypeName = "decimal(12,2)")]
        public decimal BasePrice { get; set; }

        [Column("sale_price", TypeName = "decimal(12,2)")]
        public decimal? SalePrice { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
        [Column("img")]
        public string? Image { get; set; }
        [Column("img_public_id")]
        public string? ImagePublicId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation Properties ──────────────────────────────
        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; } = null!;

        [ForeignKey(nameof(BrandId))]
        public Brand Brand { get; set; } = null!;

       
    }
}

