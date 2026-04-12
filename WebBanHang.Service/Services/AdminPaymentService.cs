using System;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.Service.Exceptions;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Payment;
using WebBanHang.Service.IServices;

namespace WebBanHang.Service.Services
{
    public class AdminPaymentService : IAdminPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminPaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AdminPaymentListResponseDto> GetPaymentsAsync(AdminPaymentQueryDto queryDto)
        {
            var page = queryDto.Page <= 0 ? 1 : queryDto.Page;
            var pageSize = queryDto.PageSize <= 0 ? 10 : queryDto.PageSize;

            var payments = await _unitOfWork.Payment.GetAllAsync(includeProperties: "Order.User");
            var query = payments.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(queryDto.Status))
            {
                if (!TryNormalizePaymentStatus(queryDto.Status, out var normalizedStatus))
                {
                    throw new ApiException("Trạng thái thanh toán không hợp lệ.", 400);
                }

                query = query.Where(x => string.Equals(NormalizePaymentStatusLabel(x.PaymentStatus), normalizedStatus, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.Method))
            {
                if (!TryNormalizePaymentMethod(queryDto.Method, out var normalizedMethod))
                {
                    throw new ApiException("Phương thức thanh toán không hợp lệ.", 400);
                }

                query = query.Where(x => string.Equals(NormalizePaymentMethodLabel(x.PaymentMethod), normalizedMethod, StringComparison.OrdinalIgnoreCase));
            }

            var total = query.Count();
            var items = query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AdminPaymentListItemDto
                {
                    Id = NormalizePaymentId(x.TransactionCode, x.PaymentId),
                    OrderId = x.OrderId,
                    CustomerName = x.Order?.User?.FullName ?? string.Empty,
                    Amount = x.Amount,
                    Method = NormalizePaymentMethodLabel(x.PaymentMethod),
                    Status = NormalizePaymentStatusLabel(x.PaymentStatus),
                    CreatedAt = x.CreatedAt
                })
                .ToList();

            return new AdminPaymentListResponseDto
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        private static string NormalizePaymentMethodLabel(string? rawMethod)
        {
            var method = rawMethod?.Trim().ToLowerInvariant();
            return method switch
            {
                "cod" => "COD",
                "banking" => "Banking",
                "bank_transfer" => "Banking",
                "banktransfer" => "Banking",
                _ => rawMethod?.Trim() ?? string.Empty
            };
        }

        private static string NormalizePaymentStatusLabel(string? rawStatus)
        {
            var status = rawStatus?.Trim().ToLowerInvariant();
            return status switch
            {
                "paid" => "Paid",
                "success" => "Paid",
                "pending" => "Pending",
                "unpaid" => "Pending",
                "refunded" => "Refunded",
                _ => rawStatus?.Trim() ?? string.Empty
            };
        }

        private static bool TryNormalizePaymentMethod(string? rawMethod, out string normalizedMethod)
        {
            normalizedMethod = NormalizePaymentMethodLabel(rawMethod);
            return normalizedMethod is "COD" or "Banking";
        }

        private static bool TryNormalizePaymentStatus(string? rawStatus, out string normalizedStatus)
        {
            normalizedStatus = NormalizePaymentStatusLabel(rawStatus);
            return normalizedStatus is "Paid" or "Pending" or "Refunded";
        }

        private static string NormalizePaymentId(string? transactionCode, long paymentId)
        {
            if (!string.IsNullOrWhiteSpace(transactionCode))
            {
                return transactionCode.Trim();
            }

            return $"PAY-{paymentId:D3}";
        }
    }
}
