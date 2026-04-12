using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Payment;

namespace WebBanHang.Service.IServices
{
    public interface IAdminPaymentService
    {
        Task<AdminPaymentListResponseDto> GetPaymentsAsync(AdminPaymentQueryDto queryDto);
    }
}
