using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Model;

using WebBanHang.Model.Enums;

namespace WebBanHang.Model
{
    [Table("inventory_movements")]
    public class InventoryMovement
    {
        [Key]
        [Column("movement_id")]
        public long MovementId { get; set; }

        /// <summary>
        /// FK đến product_variants
        /// Ghi nhật ký kho cho từng variant cụ thể
        /// </summary>
        [Column("variant_id")]
        public long VariantId { get; set; }

        /// <summary>
        /// Loại thay đổi kho:
        /// ├── IN     → Nhập hàng từ nhà cung cấp
        /// ├── OUT    → Xuất hàng (bán, đổi trả...)
        /// └── ADJUST → Điều chỉnh thủ công sau kiểm kho
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("movement_type")]
        public string MovementType { get; set; } = InventoryMovementType.IN.ToString();

        /// <summary>
        /// Số lượng thay đổi (LUÔN dương)
        /// Dấu +/- được xác định bởi MovementType:
        /// ├── IN / ADJUST tăng → +quantity
        /// └── OUT / ADJUST giảm → -quantity
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        [Column("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Loại tài liệu gốc tạo ra movement này:
        /// ├── "order"            → xuất do đặt hàng
        /// ├── "return"           → nhập lại do hoàn hàng
        /// ├── "purchase_order"   → nhập từ nhà cung cấp
        /// └── "manual_adjust"    → nhân viên kho điều chỉnh tay
        /// </summary>
        [MaxLength(50)]
        [Column("reference_type")]
        public string? ReferenceType { get; set; }

        /// <summary>
        /// ID của tài liệu gốc (OrderId, PurchaseOrderId...)
        /// Kết hợp với ReferenceType để tra cứu nguồn gốc
        /// </summary>
        [Column("reference_id")]
        public long? ReferenceId { get; set; }

        /// <summary>
        /// Ghi chú thêm từ nhân viên kho
        /// VD: "Nhập hàng đợt 3 tháng 3/2026"
        /// </summary>
        [MaxLength(255)]
        [Column("note")]
        public string? Note { get; set; }

        /// <summary>
        /// Nhân viên thực hiện thao tác này
        /// FK đến bảng users
        /// </summary>
        [Column("created_by")]
        public long CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation Properties ──────────────────────────
        [ForeignKey(nameof(VariantId))]
        public ProductVariant ProductVariant { get; set; } = null!;

        [ForeignKey(nameof(CreatedBy))]
        public User CreatedByUser { get; set; } = null!;
    }
}
