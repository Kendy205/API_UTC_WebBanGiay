using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Model
{
    [Table("colors")]
    public class Color
    {
        [Key]
        [Column("color_id")]
        public int ColorId { get; set; }

        /// <summary>
        /// Tên màu hiển thị
        /// VD: "Đen", "Trắng", "Đỏ", "Xanh Navy"
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("color_name")]
        public string ColorName { get; set; } = string.Empty;

        /// <summary>
        /// Mã màu HEX để render trực tiếp trên UI
        /// VD: "#000000", "#FFFFFF", "#FF0000"
        /// </summary>
        [MaxLength(7)]
        [Column("hex_code")]
        public string? HexCode { get; set; }

        // ── Navigation Properties ──────────────────────────
        public ICollection<ProductVariant> ProductVariants { get; set; }
            = new List<ProductVariant>();
    }
}

