using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.Service.IServices
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetAllAsync();
        Task<PaymentDto?> GetByIdAsync(long id);
        Task AddAsync(PaymentDto dto);
        Task UpdateAsync(long id, PaymentDto dto);
        Task DeleteAsync(long id);
        string CreateVnPayPaymentUrl(OrderDto order, HttpContext context );
        Task<ApiResponse<PaymentDto>> ProcessVnPayReturn(IQueryCollection collections);
    }
}
