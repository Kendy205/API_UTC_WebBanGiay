namespace WebBanHang.Service.DTOs.Model {
    public class AddressDto {
        public long AddressId { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? StreetAddress { get; set; }
        public bool IsDefault { get; set; }
    }
}
