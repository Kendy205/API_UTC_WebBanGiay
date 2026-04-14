using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Payment;
using WebBanHang.Service.IServices;

namespace WebBanHang.Service.Services
{
    public class AdminPaymentsService : IAdminPaymentsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminPaymentsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AdminPaymentListResponseDto> GetPaymentsAsync(AdminPaymentQueryDto queryDto)
        {
            var entities = await _unitOfWork.Payment.GetAllAsync(includeProperties: "Order.User");
            var query = entities.AsEnumerable();

            // 1. Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(queryDto.Status) && TryNormalizeStatus(queryDto.Status, out var normStatus))
                query = query.Where(x => NormalizeStatusLabel(x.PaymentStatus) == normStatus);

            // 2. Lọc theo phương thức
            if (!string.IsNullOrWhiteSpace(queryDto.Method) && TryNormalizeMethod(queryDto.Method, out var normMethod))
                query = query.Where(x => NormalizeMethodLabel(x.PaymentMethod) == normMethod);

            // 3. Tìm kiếm (Mã GD hoặc Tên khách)
            if (!string.IsNullOrWhiteSpace(queryDto.Search))
            {
                var s = queryDto.Search.Trim();
                query = query.Where(x => (x.TransactionCode != null && x.TransactionCode.Contains(s, StringComparison.OrdinalIgnoreCase)) || 
                                        (x.Order?.User != null && x.Order.User.FullName.Contains(s, StringComparison.OrdinalIgnoreCase)));
            }

            // 4. Lọc ngày tháng
            if (queryDto.StartDate.HasValue) query = query.Where(x => x.CreatedAt >= queryDto.StartDate.Value);
            if (queryDto.EndDate.HasValue) query = query.Where(x => x.CreatedAt <= queryDto.EndDate.Value);

            var total = query.Count();
            var page = queryDto.Page <= 0 ? 1 : queryDto.Page;
            var pageSize = queryDto.PageSize <= 0 ? 10 : queryDto.PageSize;

            var items = query.OrderByDescending(x => x.CreatedAt)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .Select(MapToListItem)
                             .ToList();

            return new AdminPaymentListResponseDto
            {
                Data = items, Total = total, Page = page, PageSize = pageSize
            };
        }

        #region Helpers

        private static AdminPaymentListItemDto MapToListItem(WebBanHang.Model.Payment p) => new AdminPaymentListItemDto
        {
            Id = !string.IsNullOrWhiteSpace(p.TransactionCode) ? p.TransactionCode.Trim() : $"PAY-{p.PaymentId:D3}",
            OrderId = p.OrderId,
            CustomerName = p.Order?.User?.FullName ?? "N/A",
            Amount = p.Amount,
            Method = NormalizeMethodLabel(p.PaymentMethod),
            Status = NormalizeStatusLabel(p.PaymentStatus),
            CreatedAt = p.CreatedAt
        };

        private static string NormalizeMethodLabel(string? r) => r?.ToLower() switch { "cod" => "COD", "banking" => "Banking", "vnpay" => "Banking", _ => r ?? "N/A" };
        
        private static string NormalizeStatusLabel(string? r) => r?.ToLower() switch { "paid" => "Paid", "success" => "Paid", "refunded" => "Refunded", _ => "Pending" };

        private static bool TryNormalizeMethod(string? r, out string n)
        {
            n = NormalizeMethodLabel(r);
            return n is "COD" or "Banking";
        }

        private static bool TryNormalizeStatus(string? r, out string n)
        {
            n = NormalizeStatusLabel(r);
            return n is "Paid" or "Pending" or "Refunded";
        }

        #endregion
    }
}
