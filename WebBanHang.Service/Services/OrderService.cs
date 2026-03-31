using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.Service.IServices;
using WebBanHang.Model;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.DTOs.Common;

namespace WebBanHang.Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<IEnumerable<OrderDto>>> GetAllAsync()
        {
            var entities = await _unitOfWork.Order.GetAllAsync(includeProperties: "User,OrderItems");
            var dtos = _mapper.Map<IEnumerable<OrderDto>>(entities);
            return ApiResponse<IEnumerable<OrderDto>>.Succeeded(dtos);
        }

        public async Task<ApiResponse<OrderDto?>> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == id, includeProperties: "User,OrderItems,ShippingAddress");
            if (entity == null)
                return ApiResponse<OrderDto?>.Failed("Order not found", 404);
            
            var dto = _mapper.Map<OrderDto>(entity);
            return ApiResponse<OrderDto?>.Succeeded(dto);
        }

        public async Task<ApiResponse<IEnumerable<OrderDto>>> GetByUserIdAsync(long userId)
        {
            var entities = await _unitOfWork.Order.GetAllAsync(x => x.UserId == userId, includeProperties: "OrderItems");
            var dtos = _mapper.Map<IEnumerable<OrderDto>>(entities);
            return ApiResponse<IEnumerable<OrderDto>>.Succeeded(dtos);
        }

        public async Task<ApiResponse<OrderDto>> PlaceOrderAsync(CheckoutDto checkoutDto)
        {
            // 1. Verify User
            var user = await _unitOfWork.User.GetFirstOrDefaultAsync(u => u.UserId == checkoutDto.UserId);
            if (user == null) return ApiResponse<OrderDto>.Failed("User not found", 404);

            // 2. Verify Address
            var address = await _unitOfWork.Address.GetFirstOrDefaultAsync(a => a.AddressId == checkoutDto.ShippingAddressId && a.UserId == checkoutDto.UserId);
            if (address == null) return ApiResponse<OrderDto>.Failed("Shipping address not found or does not belong to user", 400);

            if (checkoutDto.Items == null || !checkoutDto.Items.Any())
                return ApiResponse<OrderDto>.Failed("Order must have at least one item", 400);

            var order = new Order
            {
                UserId = checkoutDto.UserId,
                ShippingAddressId = checkoutDto.ShippingAddressId,
                OrderCode = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999),
                OrderStatus = "pending",
                PaymentStatus = "unpaid",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SubtotalAmount = 0,
                TotalAmount = 0
            };

            decimal subtotal = 0;
            var inventoryMovements = new List<InventoryMovement>();

            foreach (var item in checkoutDto.Items)
            {
                var variant = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(
                    v => v.VariantId == item.VariantId, 
                    includeProperties: "Product,Size,Color");

                if (variant == null)
                    return ApiResponse<OrderDto>.Failed($"Product variant with ID {item.VariantId} not found", 400);

                if (variant.StockQuantity < item.Quantity)
                    return ApiResponse<OrderDto>.Failed($"Not enough stock for {variant.Product.ProductName}. Available: {variant.StockQuantity}", 400);

                var unitPrice = variant.PriceOverride ?? variant.Product.SalePrice ?? variant.Product.BasePrice;
                var lineTotal = unitPrice * item.Quantity;

                var orderItem = new OrderItem
                {
                    VariantId = item.VariantId,
                    ProductNameSnapshot = variant.Product.ProductName,
                    SizeLabelSnapshot = variant.Size.SizeLabel,
                    ColorNameSnapshot = variant.Color.ColorName,
                    SkuSnapshot = variant.Sku,
                    UnitPrice = unitPrice,
                    Quantity = item.Quantity,
                    LineTotal = lineTotal
                };

                order.OrderItems.Add(orderItem);
                subtotal += lineTotal;

                // Update stock
                variant.StockQuantity -= item.Quantity;
                _unitOfWork.ProductVariant.Update(variant);

                // Prepare inventory movement
                inventoryMovements.Add(new InventoryMovement
                {
                    VariantId = item.VariantId,
                    MovementType = "OUT",
                    Quantity = item.Quantity,
                    ReferenceType = "order",
                    Note = $"Order {order.OrderCode} placed",
                    CreatedBy = checkoutDto.UserId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            order.SubtotalAmount = subtotal;
            order.TotalAmount = subtotal + order.ShippingFee - order.DiscountAmount;

            await _unitOfWork.Order.AddAsync(order);
            await _unitOfWork.SaveAsync(); // Save to get OrderId

            // Update inventory movements with OrderId
            foreach (var move in inventoryMovements)
            {
                move.ReferenceId = order.OrderId;
                await _unitOfWork.InventoryMovement.AddAsync(move);
            }
            await _unitOfWork.SaveAsync();

            var resultDto = _mapper.Map<OrderDto>(order);
            resultDto.CustomerName = user.FullName;

            return ApiResponse<OrderDto>.Succeeded(resultDto, "Order placed successfully", 201);
        }

        public async Task<ApiResponse<bool>> UpdateOrderStatusAsync(long orderId, string status)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return ApiResponse<bool>.Failed("Order not found", 404);

            order.OrderStatus = status;
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();

            return ApiResponse<bool>.Succeeded(true, $"Order status updated to {status}");
        }

        public async Task<ApiResponse<bool>> UpdatePaymentStatusAsync(long orderId, string status)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return ApiResponse<bool>.Failed("Order not found", 404);

            order.PaymentStatus = status;
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();

            return ApiResponse<bool>.Succeeded(true, $"Payment status updated to {status}");
        }

        public async Task<ApiResponse<bool>> CancelOrderAsync(long orderId)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == orderId, includeProperties: "OrderItems");
            if (order == null) return ApiResponse<bool>.Failed("Order not found", 404);

            if (order.OrderStatus == "cancelled")
                return ApiResponse<bool>.Failed("Order is already cancelled", 400);

            if (order.OrderStatus == "shipped" || order.OrderStatus == "delivered")
                return ApiResponse<bool>.Failed("Cannot cancel order that has been shipped or delivered", 400);

            order.OrderStatus = "cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            // Restock items
            foreach (var item in order.OrderItems)
            {
                var variant = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(v => v.VariantId == item.VariantId);
                if (variant != null)
                {
                    variant.StockQuantity += item.Quantity;
                    _unitOfWork.ProductVariant.Update(variant);

                    // Add inventory movement for restock
                    var move = new InventoryMovement
                    {
                        VariantId = item.VariantId,
                        MovementType = "IN",
                        Quantity = item.Quantity,
                        ReferenceType = "order",
                        ReferenceId = order.OrderId,
                        Note = $"Order {order.OrderCode} cancelled - restocked",
                        CreatedBy = order.UserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.InventoryMovement.AddAsync(move);
                }
            }

            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();

            return ApiResponse<bool>.Succeeded(true, "Order cancelled successfully");
        }

        public async Task<ApiResponse<bool>> DeleteAsync(long id)
        {
            var entity = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == id);
            if (entity == null)
                return ApiResponse<bool>.Failed("Order not found", 404);

            _unitOfWork.Order.Remove(entity);
            await _unitOfWork.SaveAsync();
            return ApiResponse<bool>.Succeeded(true, "Order deleted successfully");
        }
    }
}
