using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Payment;

namespace WebBanHang.Service.IServices
{
    public interface IAdminPaymentsService
    {
        Task<AdminPaymentListResponseDto> GetPaymentsAsync(
          string? status,
          string? method,
          string? search,
          DateTime? startDate,
          DateTime? endDate,
          int page,
          int pageSize);
    }

}
