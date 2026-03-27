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
    [Table("order_items")]
    public class OrderItem
    {
        [Key]
        [Column("order_item_id")]
        public long OrderItemId { get; set; }

        [Column("order_id")]
        public long OrderId { get; set; }

        [Column("variant_id")]
        public long VariantId { get; set; }

        /// <summary>Snapshot tên sản phẩm tại thời điểm mua</summary>
        [MaxLength(255)]
        [Column("product_name_snapshot")]
        public string ProductNameSnapshot { get; set; } = string.Empty;

        /// <summary>Snapshot size tại thời điểm mua</summary>
        [MaxLength(20)]
        [Column("size_label_snapshot")]
        public string? SizeLabelSnapshot { get; set; }

        /// <summary>Snapshot màu tại thời điểm mua</summary>
        [MaxLength(50)]
        [Column("color_name_snapshot")]
        public string? ColorNameSnapshot { get; set; }

        /// <summary>Snapshot SKU tại thời điểm mua</summary>
        [MaxLength(100)]
        [Column("sku_snapshot")]
        public string? SkuSnapshot { get; set; }

        [Column("unit_price", TypeName = "decimal(12,2)")]
        public decimal UnitPrice { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("line_total", TypeName = "decimal(12,2)")]
        public decimal LineTotal { get; set; }

        // ── Navigation Properties ──────────────────────────────
        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = null!;

        [ForeignKey(nameof(VariantId))]
        public ProductVariant ProductVariant { get; set; } = null!;

        public Review? Review { get; set; }
    }
}

