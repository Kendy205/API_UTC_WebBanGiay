using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Service.DTOs.Model {
    public class BrandDto {
       
        public long BrandId { get; set; }

        public string BrandName { get; set; } = string.Empty;

    
        public string? Slug { get; set; }

        public bool IsActive { get; set; } = true;

       
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
