using System;
using System.Threading.Tasks;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Payment;

namespace WebBanHang.Service.IServices
{
    public interface IAdminPaymentsService
    {
        Task<PagedResult<AdminPaymentListItemDto>> GetPaymentsAsync(
          string? status,
          string? method,
          string? search,
          DateTime? startDate,
          DateTime? endDate,
          int page,
          int pageSize);
    }

}