using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Model.Enums;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.IServices;

namespace WebBanHang.Service.Services
{
    public class MyOrdersService : IMyOrdersService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAddressService _addressService;
        private readonly IInventoryMovementService _inventoryMovementService;
        private readonly ICartService _cartService;
        private readonly ICartItemService _cartItemService;
        public MyOrdersService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IAddressService addressService,
            IInventoryMovementService inventoryMovementService,
            ICartService cartService,
            ICartItemService cartItemService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _addressService = addressService;
            _inventoryMovementService = inventoryMovementService;
            _cartService = cartService;
            _cartItemService = cartItemService;
        }

        // ================= GET METHODS =================

        public async Task<PagedResult<OrderDto>> GetMyOrdersAsync(long currentUserId, int pageNumber, int pageSize)
        {
            // Đảm bảo các tham số phân trang có giá trị hợp lệ
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var entities = await _unitOfWork.Order.GetAllAsync(
                filter: x => x.UserId == currentUserId,
                includeProperties: "OrderItems,ShippingAddress,OrderItems.ProductVariant.Product,OrderItems.ProductVariant.Size,OrderItems.ProductVariant.Color",
                pageSize: pageSize,
                pageNumber: pageNumber
            );

            // 1. Đếm tổng số đơn hàng của người dùng hiện tại
            var totalCount = await _unitOfWork.Order.CountAsync(x => x.UserId == currentUserId);

            // 2. Trả về cấu trúc PagedResult
            return new PagedResult<OrderDto>
            {
                Data = _mapper.Map<IEnumerable<OrderDto>>(entities),
                Total = totalCount,
                Page = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<OrderDto?> GetMyOrderByIdAsync(long orderId, long currentUserId)
        {
            var entity = await _unitOfWork.Order.GetFirstOrDefaultAsync(
                x => x.OrderId == orderId && x.UserId == currentUserId,
                includeProperties: "OrderItems,ShippingAddress,OrderItems.ProductVariant.Product,OrderItems.ProductVariant.Size,OrderItems.ProductVariant.Color"
            );

            return entity == null ? null : _mapper.Map<OrderDto>(entity);
        }

        // ================= CHECKOUT (PIPELINE) =================

        public async Task<OrderDto> CheckoutAsync(CheckoutDto checkoutDto, long currentUserId)
        {
            // 1. Dùng CartService lấy thông tin giỏ hàng (Thay cho dùng UnitOfWork trực tiếp)
            var cartDto = await _cartService.GetCartByUserId(currentUserId);

            if (cartDto == null || cartDto.Items == null || !cartDto.Items.Any())
                throw new Exception("Giỏ hàng không tồn tại hoặc đang trống.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 2. Các bước giải quyết địa chỉ, tạo header đơn hàng (Giữ nguyên logic của bạn)
                var addressId = await ResolveAddressAsync(checkoutDto, currentUserId);
                var order = CreateOrderHeader(currentUserId, addressId, checkoutDto);

                // 3. Build Items (Sử dụng danh sách items từ CheckoutDto gửi lên)
                foreach (var item in checkoutDto.Items)
                {
                    var variant = await GetVariantWithDetailsAsync(item.VariantId);
                    order.OrderItems.Add(BuildOrderItemSnapshot(variant, item.Quantity));
                }

                // 4. Lưu đơn hàng và xử lý kho
                FinalizeOrderTotals(order);
                await _unitOfWork.Order.AddAsync(order);
                await _unitOfWork.SaveAsync();

                await _inventoryMovementService.HandleCheckoutAsync(order, checkoutDto.Items, currentUserId);

                // 5. DỌN DẸP GIỎ HÀNG: Sử dụng hàm Clear có sẵn trong CartItemService
                // Hàm này bên trong đã có sẵn logic truy vấn database để xóa, 
                // bạn chỉ cần truyền CartId vào là xong, cực kỳ sạch sẽ.
                await _cartItemService.ClearCartAsync(cartDto.CartId);

                await transaction.CommitAsync();
                return _mapper.Map<OrderDto>(order);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // ================= CANCEL ORDER =================

        public async Task<bool> CancelMyOrderAsync(long orderId, long currentUserId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(
                    x => x.OrderId == orderId && x.UserId == currentUserId,
                    includeProperties: "OrderItems"
                );

                if (order == null) throw new Exception("Không tìm thấy đơn hàng.");

                // Kiểm tra trạng thái có được phép hủy hay không
                ValidateCancelStatus(order);

                // Hoàn lại kho thông qua dịch vụ Inventory
                await _inventoryMovementService.HandleOrderCancelAsync(order, currentUserId);

                // Cập nhật trạng thái đơn hàng
                order.OrderStatus = OrderStatus.Cancelled.ToString();
                order.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Order.Update(order);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ================= PRIVATE HELPER METHODS =================

        private async Task<long> ResolveAddressAsync(CheckoutDto dto, long userId)
        {
            // Trường hợp người dùng nhập địa chỉ mới ngay tại trang thanh toán
            if (dto.NewAddress != null)
            {
                dto.NewAddress.UserId = userId;
                await _addressService.AddAsync(dto.NewAddress);

                var addresses = await _unitOfWork.Address.GetAllAsync(x => x.UserId == userId);
                return addresses.OrderByDescending(x => x.AddressId).First().AddressId;
            }

            // Trường hợp chọn địa chỉ đã có sẵn
            if (dto.ShippingAddressId.HasValue)
            {
                var address = await _unitOfWork.Address.GetFirstOrDefaultAsync(
                    x => x.AddressId == dto.ShippingAddressId && x.UserId == userId);

                if (address == null) throw new Exception("Địa chỉ giao hàng không hợp lệ.");
                return address.AddressId;
            }

            throw new Exception("Vui lòng cung cấp địa chỉ giao hàng.");
        }

        private Order CreateOrderHeader(long userId, long addressId, CheckoutDto dto)
        {
            decimal shippingFee = dto.ShippingFee ?? (dto.DistanceKM.HasValue ? CalculateLineTotal(dto.DistanceKM.Value) : 0);
            if( shippingFee != dto.ShippingFee)
            {
                throw new Exception("Khoảng cách vận chuyển không hợp lệ hoặc không được cung cấp, vui lòng kiểm tra lại.");
            }    
            return new Order
            {
                UserId = userId,
                ShippingAddressId = addressId,
                OrderCode = $"ORD-{DateTime.UtcNow:yyMMddHHmmssfff}",
                OrderStatus = OrderStatus.Pending.ToString(),
                PaymentStatus = PaymentStatus.Unpaid.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ShippingFee = shippingFee,
                OrderItems = new List<OrderItem>()
            };
        }
        private decimal CalculateLineTotal(decimal DistanceKM)
        {
            if (DistanceKM <= 0) return 0;
            if (DistanceKM <= 5) return 15000;
            if (DistanceKM <= 15) return 25000;
            if (DistanceKM <= 30) return 35000;
            if (DistanceKM <= 50) return 50000;
            return Math.Round(50000 + (DistanceKM - 50) * 800);
        }
        private async Task<ProductVariant> GetVariantWithDetailsAsync(long variantId)
        {
            var variant = await _unitOfWork.ProductVariant
                .GetFirstOrDefaultAsync(x => x.VariantId == variantId, "Product,Size,Color");

            if (variant == null) throw new Exception("Sản phẩm không tồn tại trong hệ thống.");
            return variant;
        }

        private OrderItem BuildOrderItemSnapshot(ProductVariant variant, int quantity)
        {
            // Đảm bảo lấy giá hợp lệ (>0)
            var price = variant.PriceOverride ?? (decimal)(variant.Product.SalePrice > 0 ? variant.Product.SalePrice : variant.Product.BasePrice);

            return new OrderItem
            {
                VariantId = variant.VariantId,
                ProductNameSnapshot = variant.Product.ProductName,
                SizeLabelSnapshot = variant.Size?.SizeLabel,
                ColorNameSnapshot = variant.Color?.ColorName,
                SkuSnapshot = variant.Sku,
                UnitPrice = price, // Giá này cực kỳ quan trọng
                Quantity = quantity,
                LineTotal = price * quantity
            };
        }

        private void FinalizeOrderTotals(Order order)
        {
            var subtotal = order.OrderItems.Sum(x => x.LineTotal);
            order.SubtotalAmount = subtotal;
            order.TotalAmount = subtotal + order.ShippingFee;
        }

        private void ValidateCancelStatus(Order order)
        {
            if (order.OrderStatus == OrderStatus.Cancelled.ToString())
                throw new Exception("Đơn hàng này đã được hủy trước đó.");

            // Chỉ cho phép hủy khi đơn hàng đang chờ hoặc đã xác nhận nhưng chưa giao/đóng gói
            if (order.OrderStatus != OrderStatus.Pending.ToString().ToLower() &&
                order.OrderStatus != OrderStatus.Confirmed.ToString().ToLower())
            {
                throw new Exception("Đơn hàng đang trong quá trình vận chuyển, không thể hủy.");
            }
        }


    }
}