using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface IProductVariantService
    {
        Task<IEnumerable<ProductVariantDto>> GetAllAsync();
        Task<ProductVariantDto?> GetByIdAsync(long id);
        Task<IEnumerable<ProductVariantDto>> GetByProductIdAsync(long productId);
        Task AddAsync(ProductVariantDto dto);
        Task UpdateAsync(long id, ProductVariantDto dto);
        Task DeleteAsync(long id);
    }
}
