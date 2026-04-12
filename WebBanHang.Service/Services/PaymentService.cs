using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Common;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.Helpers.VnPay;
using WebBanHang.Service.IServices;

namespace WebBanHang.Service.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<IEnumerable<PaymentDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.Payment.GetAllAsync();
            return _mapper.Map<IEnumerable<PaymentDto>>(entities);
        }

        public async Task<PaymentDto?> GetByIdAsync(long id)
        {
            // Tạm thời gọi GetFirstOrDefaultAsync, bạn nhớ truyền biểu thức lambda khớp với tên khóa chính (ví dụ x => x.PaymentId == id) vào nhé.
            var entity = await _unitOfWork.Payment.GetFirstOrDefaultAsync(x => x.PaymentId == id);
            return _mapper.Map<PaymentDto>(entity); // TODO: Cập nhật lại biểu thức tìm kiếm ID tại đây
        }

        public async Task AddAsync(PaymentDto dto)
        {
            var entity = _mapper.Map<Payment>(dto);
            await _unitOfWork.Payment.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, PaymentDto dto)
        {
            // TODO: Tìm entity cũ theo id, sau đó map đè dữ liệu
            var entity = await _unitOfWork.Payment.GetFirstOrDefaultAsync(x => x.PaymentId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.Payment.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            // TODO: Tìm entity cũ theo id, sau đó xóa
            var entity = await _unitOfWork.Payment.GetFirstOrDefaultAsync(x => x.PaymentId == id);
            if (entity != null)
            {
                _unitOfWork.Payment.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public string CreateVnPayPaymentUrl(OrderDto order, HttpContext context)
        {
            var tmnCode = _configuration["Vnpay:TmnCode"];
            var hashSecret = _configuration["Vnpay:HashSecret"];
            var baseUrl = _configuration["Vnpay:BaseUrl"];
            var returnUrl = _configuration["Vnpay:ReturnUrl"];

            var vnpay = new VnPayLibrary(); // Lớp Helper của VNPay

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", tmnCode);
            // VNPay yêu cầu số tiền nhân với 100
            vnpay.AddRequestData("vnp_Amount", (order.TotalAmount * 100).ToString("0"));
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang: " + order.OrderCode);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", order.OrderId.ToString());
            //vnpay.AddRequestData("vnp_Amount", (order.TotalAmount * 100).ToString("0"));
            //vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang " + order.OrderCode);
            //vnpay.AddRequestData("vnp_TxnRef", order.OrderId.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(baseUrl, hashSecret);
            return paymentUrl;
        }

        public async Task<ApiResponse<PaymentDto>> ProcessVnPayReturn(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            var vnp_orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
            var vnp_SecureHash = collections["vnp_SecureHash"];
            var hashSecret = _configuration["Vnpay:HashSecret"];

            // 1. Kiểm tra chữ ký bảo mật
            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, hashSecret);
            if (!checkSignature)
            {
                return ApiResponse<PaymentDto>.Failed("Chữ ký bảo mật không hợp lệ!");
            }

            // 2. Lấy đơn hàng từ DB
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == vnp_orderId);
            if (order == null)
            {
                return ApiResponse<PaymentDto>.Failed("Không tìm thấy thông tin đơn hàng.");
            }

            // 3. Kiểm tra mã phản hồi từ VNPay (00 = Thành công)
            if (vnpay.GetResponseData("vnp_ResponseCode") == "00")
            {
                // Cập nhật trạng thái đơn hàng
                order.PaymentStatus = "Paid";
                order.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Order.Update(order);

                // Lưu lịch sử thanh toán vào bảng payments
                var paymentRecord = new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethod = "VNPay",
                    TransactionCode = vnpay.GetResponseData("vnp_TransactionNo"),
                    Amount = order.TotalAmount,
                    PaymentStatus = "Success",
                    PaidAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Payment.AddAsync(paymentRecord);
                await _unitOfWork.SaveAsync(); // Lưu tất cả thay đổi cùng lúc

                var dto = _mapper.Map<PaymentDto>(paymentRecord);
                return ApiResponse<PaymentDto>.Succeeded(dto, "Thanh toán thành công.");
            }

            return ApiResponse<PaymentDto>.Failed("Giao dịch thanh toán thất bại hoặc bị hủy.");
        }
    }
}