using System;

namespace WebBanHang.Service.DTOs.Model
{
    public class InventoryMovementDto
    {
        public long MovementId { get; set; }
        public long VariantId { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? ReferenceType { get; set; }
        public long? ReferenceId { get; set; }
        public string? Note { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
