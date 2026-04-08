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
    [Table("product_variants")]
    public class ProductVariant
    {
        [Key]
        [Column("variant_id")]
        public long VariantId { get; set; }

        /// <summary>
        /// FK đến bảng products
        /// Một sản phẩm có nhiều variants
        /// </summary>
        [Column("product_id")]
        public long ProductId { get; set; }

        /// <summary>
        /// FK đến bảng sizes
        /// Size của variant này (VD: 40, 41, 42)
        /// </summary>
        [Column("size_id")]
        public int SizeId { get; set; }

        /// <summary>
        /// FK đến bảng colors
        /// Màu của variant này (VD: Đen, Trắng)
        /// </summary>
        [Column("color_id")]
        public int ColorId { get; set; }

        /// <summary>
        /// SKU = mã định danh duy nhất cho từng variant
        /// VD: "NK-AM-40-BLK"
        /// Dùng để tra kho, quét barcode
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("sku")]
        public string Sku { get; set; } = string.Empty;

        /// <summary>
        /// Giá riêng của variant (nếu null thì dùng base_price của product)
        /// VD: Size lớn hơn có thể giá cao hơn
        /// </summary>
        [Column("price_override", TypeName = "decimal(12,2)")]
        public decimal? PriceOverride { get; set; }

        /// <summary>
        /// Số lượng tồn kho hiện tại
        /// Được cập nhật qua InventoryMovements
        /// </summary>
        [Column("stock_quantity")]
        public int StockQuantity { get; set; } = 0;

        /// <summary>
        /// Khi StockQuantity dưới ngưỡng này thì cảnh báo hết hàng
        /// VD: low_stock_threshold = 5 → cảnh báo khi còn ≤ 5
        /// </summary>
        [Column("low_stock_threshold")]
        public int LowStockThreshold { get; set; } = 5;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation Properties ──────────────────────────
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;

        [ForeignKey(nameof(SizeId))]
        public Size Size { get; set; } = null!;

        [ForeignKey(nameof(ColorId))]
        public Color Color { get; set; } = null!;

        /// <summary>
        /// Các lần thêm/bớt tồn kho của variant này
        /// </summary>
        public ICollection<InventoryMovement> InventoryMovements { get; set; }
            = new List<InventoryMovement>();

        /// <summary>
        /// Các dòng trong giỏ hàng chứa variant này
        /// </summary>
        public ICollection<CartItem> CartItems { get; set; }
            = new List<CartItem>();

        /// <summary>
        /// Các dòng trong đơn hàng chứa variant này
        /// </summary>
        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}

