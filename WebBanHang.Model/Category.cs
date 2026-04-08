using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Model
{
    [Table("categories")]
    public class Category
    {
        [Key]
        [Column("category_id")]
        public long CategoryId { get; set; }

        /// <summary>Danh mục cha, hỗ trợ cây danh mục đệ quy</summary>
        [Column("parent_id")]
        public long? ParentId { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("category_name")]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(180)]
        [Column("slug")]
        public string? Slug { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation Properties ──────────────────────────────
        [ForeignKey(nameof(ParentId))]
        public Category? ParentCategory { get; set; }

        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

