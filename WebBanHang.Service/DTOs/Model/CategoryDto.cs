using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Service.DTOs.Model {
    public class CategoryDto {
        // TODO: Thêm các property cần thiết trả về cho API
       
        public long CategoryId { get; set; }

        /// <summary>Danh mục cha, hỗ trợ cây danh mục đệ quy</summary>
 
        public long? ParentId { get; set; }

 
        public string CategoryName { get; set; } = string.Empty;

       
        public string? Slug { get; set; }

        public bool IsActive { get; set; } = true;

      
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
