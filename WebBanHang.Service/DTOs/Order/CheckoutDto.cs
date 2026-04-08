using System.Collections.Generic;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.DTOs.Order
{
    public class CheckoutDto
    {
        // Dùng khi Admin đặt hàng hộ cho User khác (tùy chọn)
        public long? UserId { get; set; }

        // Nếu dùng địa chỉ cũ, truyền Id này
        public long? ShippingAddressId { get; set; }

        // Nếu tạo địa chỉ mới ngay khi checkout, truyền đối tượng này
        public AddressDto? NewAddress { get; set; }

        public string? Note { get; set; }
        public List<CartItemLocalDto> Items { get; set; } = new();
    }

    public class CartItemLocalDto
    {
        public long VariantId { get; set; }
        public int Quantity { get; set; }
    }
}
