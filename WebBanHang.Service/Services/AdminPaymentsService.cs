using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Payment;
using WebBanHang.Service.IServices;

namespace WebBanHang.Service.Services
{
    public class AdminPaymentsService : IAdminPaymentsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminPaymentsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AdminPaymentListResponseDto> GetPaymentsAsync(
            string? status,
            string? method,
            string? search,
            DateTime? startDate,
            DateTime? endDate,
            int page,
            int pageSize)
        {
            // 1. Chuẩn hóa tham số phân trang
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            // 2. Xây dựng bộ lọc động (Predicate)
            // Lưu ý: Đối với status và method, chúng ta lọc theo giá trị thô trong DB
            Expression<Func<Payment, bool>> filter = x =>
                (string.IsNullOrEmpty(status) || x.PaymentStatus.ToLower() == status.ToLower() || (status.ToLower() == "paid" && x.PaymentStatus.ToLower() == "success")) &&
                (string.IsNullOrEmpty(method) || x.PaymentMethod.ToLower() == method.ToLower() || (method.ToLower() == "banking" && x.PaymentMethod.ToLower() == "vnpay")) &&
                (string.IsNullOrEmpty(search) || x.TransactionCode.Contains(search) || x.Order.User.FullName.Contains(search)) &&
                (!startDate.HasValue || x.CreatedAt >= startDate.Value) &&
                (!endDate.HasValue || x.CreatedAt <= endDate.Value);

            // 3. Thực hiện truy vấn có phân trang và Include bảng liên quan
            var payments = await _unitOfWork.Payment.GetAllAsync(
                filter: filter,
                includeProperties: "Order.User",
                pageSize: pageSize,
                pageNumber: page
            );

            // 4. Lấy tổng số bản ghi để tính toán ở UI
            var totalCount = (await _unitOfWork.Payment.GetAllAsync(filter)).Count();

            // 5. Trả về kết quả đã được Map qua AutoMapper
            return new AdminPaymentListResponseDto
            {
                Data = _mapper.Map<List<AdminPaymentListItemDto>>(payments),
                Total = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}