using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Model.Enums;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.IServices;

namespace WebBanHang.Service.Services
{
    public class AdminOrdersService : IAdminOrdersService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IInventoryMovementService _inventoryMovementService;
        private readonly IAddressService _addressService;

        public AdminOrdersService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IInventoryMovementService inventoryMovementService,
            IAddressService addressService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _inventoryMovementService = inventoryMovementService;
            _addressService = addressService;
        }

        public async Task<PagedResult<AdminOrderListItemDto>> GetOrdersAsync(
            string? status, string? search, DateTime? startDate, DateTime? endDate, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            Expression<Func<Order, bool>> filter = x =>
                (string.IsNullOrEmpty(status) || x.OrderStatus.ToLower() == status.ToLower()) &&
                (string.IsNullOrEmpty(search) || x.OrderCode.Contains(search) || x.User.FullName.Contains(search)) &&
                (!startDate.HasValue || x.CreatedAt >= startDate.Value) &&
                (!endDate.HasValue || x.CreatedAt <= endDate.Value);

            var orders = await _unitOfWork.Order.GetAllAsync(filter, "User,OrderItems,Payments", pageSize, page);
            var totalCount = await _unitOfWork.Order.CountAsync(filter);

            return new PagedResult<AdminOrderListItemDto>
            {
                Data = _mapper.Map<List<AdminOrderListItemDto>>(orders),
                Total = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<AdminOrderDetailDto?> GetOrderDetailAsync(long id)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(
                x => x.OrderId == id,
                "User,ShippingAddress,OrderItems,Payments"
            );

            return order == null ? null : _mapper.Map<AdminOrderDetailDto>(order);
        }

        public async Task<AdminOrderStatusResultDto> UpdateOrderStatusAsync(long id, string status)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == id, "OrderItems");
                if (order == null) throw new Exception("Không tìm thấy đơn hàng.");

                string oldStatus = order.OrderStatus;
                order.OrderStatus = status;
                order.UpdatedAt = DateTime.UtcNow;

                // Xử lý hoàn kho nếu hủy đơn
                if (status.Equals(OrderStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase) &&
                    !oldStatus.Equals(OrderStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    await _inventoryMovementService.HandleOrderCancelAsync(order, 0);
                }

                _unitOfWork.Order.Update(order);
                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                return new AdminOrderStatusResultDto { Id = id, Status = status };
            }
            catch { await transaction.RollbackAsync(); throw; }
        }

        public async Task<OrderDto> CreateOrderAsync(CheckoutDto checkoutDto, long customerId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var addressId = await ResolveAddressAsync(checkoutDto, customerId);
                var order = CreateOrderHeader(customerId, addressId, checkoutDto);

                foreach (var item in checkoutDto.Items)
                {
                    var variant = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(x => x.VariantId == item.VariantId, "Product,Size,Color");
                    order.OrderItems.Add(BuildOrderItemSnapshot(variant!, item.Quantity));
                }

                FinalizeOrderTotals(order);
                await _unitOfWork.Order.AddAsync(order);
                await _unitOfWork.SaveAsync();

                await _inventoryMovementService.HandleCheckoutAsync(order, checkoutDto.Items, 0);

                await transaction.CommitAsync();
                return _mapper.Map<OrderDto>(order);
            }
            catch { await transaction.RollbackAsync(); throw; }
        }

        public async Task<OrderDto> UpdateOrderAsync(long orderId, OrderUpdateDto updateDto)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == orderId);
            if (order == null) throw new Exception("Đơn hàng không tồn tại.");

            _mapper.Map(updateDto, order);
            order.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<bool> DeleteOrderAsync(long orderId)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == orderId);
            if (order == null) return false;

            _unitOfWork.Order.Remove(order);
            await _unitOfWork.SaveAsync();
            return true;
        }

        // ================= PRIVATE HELPERS =================

        private async Task<long> ResolveAddressAsync(CheckoutDto dto, long userId)
        {
            if (dto.NewAddress != null)
            {
                dto.NewAddress.UserId = userId;
                await _addressService.AddAsync(dto.NewAddress);
                var addrs = await _unitOfWork.Address.GetAllAsync(x => x.UserId == userId);
                return addrs.OrderByDescending(a => a.AddressId).First().AddressId;
            }
            return dto.ShippingAddressId ?? throw new Exception("Địa chỉ là bắt buộc.");
        }

        private Order CreateOrderHeader(long userId, long addressId, CheckoutDto dto) => new Order
        {
            UserId = userId,
            ShippingAddressId = addressId,
            OrderCode = $"ADM-{DateTime.UtcNow:yyMMddHHmmssfff}",
            OrderStatus = OrderStatus.Confirmed.ToString(),
            PaymentStatus = PaymentStatus.Unpaid.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ShippingFee = dto.ShippingFee ?? 0,
            OrderItems = new List<OrderItem>()
        };

        private OrderItem BuildOrderItemSnapshot(ProductVariant v, int q)
        {
            var p = v.PriceOverride ??  (decimal)(v.Product.SalePrice >0 ? v.Product.SalePrice : v.Product.BasePrice);
            return new OrderItem
            {
                VariantId = v.VariantId,
                ProductNameSnapshot = v.Product.ProductName,
                SizeLabelSnapshot = v.Size?.SizeLabel,
                ColorNameSnapshot = v.Color?.ColorName,
                SkuSnapshot = v.Sku,
                UnitPrice = p,
                Quantity = q,
                LineTotal = p * q
            };
        }

        private void FinalizeOrderTotals(Order order)
        {
            var subtotal = order.OrderItems.Sum(x => x.LineTotal);
            order.SubtotalAmount = subtotal;
            order.TotalAmount = subtotal + order.ShippingFee - order.DiscountAmount;
        }
    }
}