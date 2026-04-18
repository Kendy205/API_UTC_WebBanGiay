using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.IServices
{
    public interface IProductService
    {
        Task<PagedResult<ProductDto>> GetAllAsync(int? pageSize,int? page);
        Task<ProductDto?> GetByIdAsync(long id);
        Task<ProductDto> AddAsync(ProductDto dto, IFormFile file);
        Task UpdateAsync(long id, ProductDto dto);
        Task DeleteAsync(long id);
        Task<PagedResult<ProductDto>> GetFilteredProductsAsync(string? keyword, long? categoryId, long? brandId, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize, string? sortBy);
    }
}
