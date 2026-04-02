namespace WebBanHang.Service.DTOs.Model
{
    public class CartItemDto
    {
        // TODO: Thêm các property cần thiết trả về cho API
        public long Id { get; set; }

        public long CartId { get; set; }

        public long ProductVariantId { get; set; }

        public int Quantity { get; set; }


    }
}
