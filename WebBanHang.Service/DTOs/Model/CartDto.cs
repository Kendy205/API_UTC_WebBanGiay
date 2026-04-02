namespace WebBanHang.Service.DTOs.Model {
    public class CartDto {
        // TODO: Thêm các property cần thiết trả về cho API
        public long CartId { get; set; } 
        public IEnumerable<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}
