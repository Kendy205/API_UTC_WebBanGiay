using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.IServices
{
    public interface IAdminProductService
    {
        Task<PagedResult<ProductAdminDto>> GetProductsAsync(string? search, int page, int pageSize);
        Task<ProductDto> GetProductByIdAsync(long id);
        //Task<ProductAdminDto> CreateAsync(ProductAdminDto dto, IFormFile file);
        Task<ProductDto> AddAsync(ProductDto dto, IFormFile file);
        Task<ProductDto> UpdateAsync(long id, ProductDto dto, IFormFile file);
        Task DeleteAsync(long id);
    }
}
