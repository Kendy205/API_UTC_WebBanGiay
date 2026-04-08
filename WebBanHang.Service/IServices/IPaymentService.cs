using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;

namespace WebBanHang.BLL.IServices
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetAllAsync();
        Task<PaymentDto?> GetByIdAsync(long id);
        Task AddAsync(PaymentDto dto);
        Task UpdateAsync(long id, PaymentDto dto);
        Task DeleteAsync(long id);
    }
}
