using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Payment;

namespace WebBanHang.Service.IServices
{
    public interface IAdminPaymentsService
    {
        Task<ApiResponse<AdminPaymentListResponseDto>> GetPaymentsAsync(AdminPaymentQueryDto queryDto);
    }
}
