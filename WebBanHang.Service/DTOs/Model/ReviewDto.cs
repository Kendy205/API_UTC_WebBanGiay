namespace WebBanHang.Service.DTOs.Model
{
    public class ReviewDto
    {
        public long ReviewId { get; set; }
        public long UserId { get; set; }
        public long OrderItemId { get; set; }
        public short Rating { get; set; }
        public string? ReviewContent { get; set; }
        public string? ReviewTitle { get; set; }
        public bool isPublic { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
