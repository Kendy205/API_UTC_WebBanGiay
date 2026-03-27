using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Model
{
    [Table("sizes")]
    public class Size
    {
        [Key]
        [Column("size_id")]
        public int SizeId { get; set; }

        /// <summary>
        /// Nhãn hiển thị cho người dùng
        /// VD: "38", "39", "40", "41", "42"
        /// Hoặc US/EU: "US7", "EU40"
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("size_label")]
        public string SizeLabel { get; set; } = string.Empty;

        /// <summary>
        /// Hệ thống kích cỡ: VN | EU | US | UK
        /// </summary>
        [MaxLength(10)]
        [Column("size_system")]
        public string? SizeSystem { get; set; } = "VN";

        /// <summary>
        /// Thứ tự sắp xếp hiển thị (38 trước 39...)
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; } = 0;

        // ── Navigation Properties ──────────────────────────
        public ICollection<ProductVariant> ProductVariants { get; set; }
            = new List<ProductVariant>();
    }
}

