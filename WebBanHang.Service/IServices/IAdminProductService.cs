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
        //Task<ProductAdminDto> CreateAsync(ProductAdminDto dto, IFormFile file);
        Task<ProductDto> AddAsync(ProductDto dto, IFormFile file);
        Task UpdateAsync(long id, ProductAdminDto dto);
        Task DeleteAsync(long id);
    }
}
