using System.Collections.Generic;

namespace WebBanHang.Service.DTOs.Order {
    public class CheckoutDto {
        public long UserId { get; set; }
        public long ShippingAddressId { get; set; }
        public List<CheckoutItemDto> Items { get; set; } = new List<CheckoutItemDto>();
        public string? Note { get; set; }
    }

    public class CheckoutItemDto {
        public long VariantId { get; set; }
        public int Quantity { get; set; }
    }
}
