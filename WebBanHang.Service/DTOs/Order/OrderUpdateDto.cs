using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.DTOs.Order
{
    public class OrderUpdateDto
    {
        public string? OrderCode { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public long? ShippingAddressId { get; set; }
        
        // Dùng để sửa sâu thông tin địa chỉ (Tên, SĐT, Địa chỉ chi tiết...)
        public AddressDto? UpdatedAddress { get; set; }

        public decimal? ShippingFee { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? SubtotalAmount { get; set; }
    }
}
