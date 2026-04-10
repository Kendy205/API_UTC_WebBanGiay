using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
using WebBanHang.Model.Enums;

namespace WebBanHang.Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IInventoryMovementService _inventoryMovementService;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IInventoryMovementService inventoryMovementService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _inventoryMovementService = inventoryMovementService;
        }

        public async Task<ApiResponse<IEnumerable<OrderDto>>> GetOrdersAsync(long userId, bool isAdmin)
        {
            IEnumerable<Order> entities;
            if (isAdmin)
            {
                entities = await _unitOfWork.Order.GetAllAsync(includeProperties: "User,OrderItems");
            }
            else
            {
                entities = await _unitOfWork.Order.GetAllAsync(x => x.UserId == userId, includeProperties: "OrderItems");
            }
            
            var dtos = _mapper.Map<IEnumerable<OrderDto>>(entities);
            return ApiResponse<IEnumerable<OrderDto>>.Succeeded(dtos);
        }

        public async Task<ApiResponse<OrderDto?>> GetByIdAsync(long id, long userId, bool isAdmin)
        {
            var entity = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == id, includeProperties: "User,OrderItems,ShippingAddress");
            if (entity == null)
                return ApiResponse<OrderDto?>.Failed("Order not found", 404);
            
            if (!isAdmin && entity.UserId != userId)
                return ApiResponse<OrderDto?>.Failed("You do not have permission to view this order", 403);

            var dto = _mapper.Map<OrderDto>(entity);
            return ApiResponse<OrderDto?>.Succeeded(dto);
        }

        public async Task<ApiResponse<OrderDto>> PlaceOrderAsync(CheckoutDto checkoutDto, long userId)
        {
            if (userId <= 0)
            {
                return ApiResponse<OrderDto>.Failed("Unauthorized", 401);
            }

            if (checkoutDto.Items == null || !checkoutDto.Items.Any())
            {
                return ApiResponse<OrderDto>.Failed("Items must not be empty.", 400);
            }

            if (checkoutDto.Items.Any(x => x.Quantity <= 0))
            {
                return ApiResponse<OrderDto>.Failed("Quantity must be greater than 0.", 400);
            }

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                async Task<ApiResponse<OrderDto>> FailAsync(string message, int statusCode = 400)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<OrderDto>.Failed(message, statusCode);
                }

                try
                {
                    // 1. Xử lý địa chỉ
                    long finalAddressId = 0;
                    if (checkoutDto.NewAddress != null)
                    {
                        var newAddress = _mapper.Map<Address>(checkoutDto.NewAddress);
                        newAddress.UserId = userId;
                        newAddress.CreatedAt = DateTime.UtcNow;
                        await _unitOfWork.Address.AddAsync(newAddress);
                        await _unitOfWork.SaveAsync();
                        finalAddressId = newAddress.AddressId;
                    }
                    else if (checkoutDto.ShippingAddressId.HasValue)
                    {
                        var existingAddr = await _unitOfWork.Address.GetFirstOrDefaultAsync(a => a.AddressId == checkoutDto.ShippingAddressId.Value && a.UserId == userId);
                        if (existingAddr == null) return await FailAsync("Invalid shipping address.", 400);
                        finalAddressId = existingAddr.AddressId;
                    }
                    else
                    {
                        return await FailAsync("Please provide a shipping address.", 400);
                    }

                    // 2. Tạo Order mới
                    var orderCode = await GenerateUniqueOrderCodeAsync();
                    if (string.IsNullOrEmpty(orderCode))
                    {
                        return await FailAsync("Could not generate a unique order code.", 409);
                    }

                    var order = new Order
                    {
                        UserId = userId,
                        ShippingAddressId = finalAddressId,
                        OrderCode = orderCode,
                        OrderStatus = WebBanHang.Model.Enums.OrderStatus.Pending.ToString(),
                        PaymentStatus = WebBanHang.Model.Enums.PaymentStatus.Unpaid.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ShippingFee = 30000
                    };

                    decimal subtotal = 0;
                    var movementDtos = new List<InventoryMovementDto>();

                    // 3. Xử lý từng item từ Giỏ hàng Local (checkoutDto.Items)
                    foreach (var itemDto in checkoutDto.Items)
                    {
                        var variant = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(
                            v => v.VariantId == itemDto.VariantId, 
                            includeProperties: "Product,Size,Color"
                        );

                        if (variant == null) return await FailAsync($"Product variant {itemDto.VariantId} does not exist.", 404);

                        var stockDecreased = await _unitOfWork.ProductVariant.TryDecreaseStockAsync(variant.VariantId, itemDto.Quantity);
                        if (!stockDecreased) return await FailAsync($"Sản phẩm {variant.Product.ProductName} đã hết hàng hoặc không đủ số lượng.", 409);

                        // Tính giá (Ưu tiên PriceOverride của Variant, sau đó đến SalePrice/BasePrice của Product)
                        decimal unitPrice = variant.PriceOverride ?? variant.Product.SalePrice ?? variant.Product.BasePrice;

                        var orderItem = new OrderItem
                        {
                            VariantId = variant.VariantId,
                            ProductNameSnapshot = variant.Product.ProductName,
                            SizeLabelSnapshot = variant.Size.SizeLabel,
                            ColorNameSnapshot = variant.Color.ColorName,
                            SkuSnapshot = variant.Sku,
                            UnitPrice = unitPrice,
                            Quantity = itemDto.Quantity,
                            LineTotal = unitPrice * itemDto.Quantity
                        };

                        order.OrderItems.Add(orderItem);
                        subtotal += orderItem.LineTotal;

                        // Tạo nhật ký kho (InventoryMovement) loại OUT
                        movementDtos.Add(new InventoryMovementDto
                        {
                            VariantId = variant.VariantId,
                            MovementType = WebBanHang.Model.Enums.InventoryMovementType.OUT.ToString(),
                            Quantity = itemDto.Quantity,
                            ReferenceType = "order",
                            Note = $"Xuất kho cho đơn hàng {order.OrderCode} (Giỏ hàng Local)",
                            CreatedBy = userId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    order.SubtotalAmount = subtotal;
                    order.TotalAmount = subtotal + order.ShippingFee - order.DiscountAmount;
                    if (order.TotalAmount < 0)
                    {
                        return await FailAsync("Total amount is invalid.", 400);
                    }

                    // Lưu Order vào CSDL
                    await _unitOfWork.Order.AddAsync(order);
                    await _unitOfWork.SaveAsync(); 

                    // Gắn OrderId cho movement rồi ghi theo lô để không SaveChanges nhiều lần.
                    foreach (var mDto in movementDtos)
                    {
                        mDto.ReferenceId = order.OrderId;
                    }
                    await _inventoryMovementService.AddRangeAsync(movementDtos);

                    // 4. (Tùy chọn) Xóa giỏ hàng trong CSDL nếu tồn tại (để đồng bộ)
                    var cart = await _unitOfWork.Cart.GetFirstOrDefaultAsync(c => c.UserId == userId);
                    if (cart != null)
                    {
                        var dbCartItems = await _unitOfWork.CartItem.GetAllAsync(ci => ci.CartId == cart.CartId);
                        foreach (var ci in dbCartItems) _unitOfWork.CartItem.Remove(ci);
                        await _unitOfWork.SaveAsync();
                    }

                    await transaction.CommitAsync();

                    return ApiResponse<OrderDto>.Succeeded(_mapper.Map<OrderDto>(order), "Đặt hàng thành công!", 201);
                }
                catch (DbUpdateException)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<OrderDto>.Failed("Order conflict detected. Please retry.", 409);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<OrderDto>.Failed("Internal server error while placing order.", 500);
                }
            }
        }

        public async Task<ApiResponse<OrderDto>> AdminUpdateOrderAsync(long id, OrderUpdateDto updateDto)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == id, includeProperties: "OrderItems");
            if (order == null) return ApiResponse<OrderDto>.Failed("Order not found", 404);

            if (!string.IsNullOrEmpty(updateDto.OrderCode)) order.OrderCode = updateDto.OrderCode;
            if (!string.IsNullOrEmpty(updateDto.OrderStatus))
            {
                if (!Enum.TryParse<WebBanHang.Model.Enums.OrderStatus>(updateDto.OrderStatus, true, out var parsedOrderStatus))
                    return ApiResponse<OrderDto>.Failed("Invalid order status.", 400);

                order.OrderStatus = parsedOrderStatus.ToString();
            }

            if (!string.IsNullOrEmpty(updateDto.PaymentStatus))
            {
                if (!Enum.TryParse<WebBanHang.Model.Enums.PaymentStatus>(updateDto.PaymentStatus, true, out var parsedPaymentStatus))
                    return ApiResponse<OrderDto>.Failed("Invalid payment status.", 400);

                order.PaymentStatus = parsedPaymentStatus.ToString();
            }
            
            if (updateDto.UpdatedAddress != null)
            {
                var newAddr = _mapper.Map<Address>(updateDto.UpdatedAddress);
                newAddr.UserId = order.UserId;
                newAddr.CreatedAt = DateTime.UtcNow;
                await _unitOfWork.Address.AddAsync(newAddr);
                await _unitOfWork.SaveAsync();
                order.ShippingAddressId = newAddr.AddressId;
            }
            else if (updateDto.ShippingAddressId.HasValue)
            {
                var existingAddr = await _unitOfWork.Address.GetFirstOrDefaultAsync(a => a.AddressId == updateDto.ShippingAddressId.Value);
                if (existingAddr == null)
                    return ApiResponse<OrderDto>.Failed("Shipping address not found.", 404);

                order.ShippingAddressId = updateDto.ShippingAddressId.Value;
            }

            if (updateDto.ShippingFee.HasValue)
            {
                if (updateDto.ShippingFee.Value < 0) return ApiResponse<OrderDto>.Failed("Shipping fee cannot be negative.", 400);
                order.ShippingFee = updateDto.ShippingFee.Value;
            }

            if (updateDto.SubtotalAmount.HasValue)
            {
                if (updateDto.SubtotalAmount.Value < 0) return ApiResponse<OrderDto>.Failed("Subtotal cannot be negative.", 400);
                order.SubtotalAmount = updateDto.SubtotalAmount.Value;
            }

            if (updateDto.DiscountAmount.HasValue)
            {
                if (updateDto.DiscountAmount.Value < 0) return ApiResponse<OrderDto>.Failed("Discount cannot be negative.", 400);
                order.DiscountAmount = updateDto.DiscountAmount.Value;
            }

            order.UpdatedAt = DateTime.UtcNow;
            order.TotalAmount = order.SubtotalAmount + order.ShippingFee - order.DiscountAmount;
            if (order.TotalAmount < 0) return ApiResponse<OrderDto>.Failed("Total amount cannot be negative.", 400);

            _unitOfWork.Order.Update(order);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException)
            {
                return ApiResponse<OrderDto>.Failed("Order update conflict detected.", 409);
            }

            return ApiResponse<OrderDto>.Succeeded(_mapper.Map<OrderDto>(order), "Order updated successfully.");
        }

        public async Task<ApiResponse<bool>> CancelOrderAsync(long orderId, long? currentUserId = null)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == orderId, includeProperties: "OrderItems");
            if (order == null) return ApiResponse<bool>.Failed("Order not found", 404);

            if (currentUserId.HasValue && order.UserId != currentUserId.Value)
                return ApiResponse<bool>.Failed("Permission denied", 403);

            // Chỉ cho phép hủy khi chưa giao
            if (order.OrderStatus == WebBanHang.Model.Enums.OrderStatus.Shipped.ToString() || order.OrderStatus == WebBanHang.Model.Enums.OrderStatus.Delivered.ToString())
                return ApiResponse<bool>.Failed("Cannot cancel order that has been shipped or delivered.", 400);

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                async Task<ApiResponse<bool>> FailAsync(string message, int statusCode = 400)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<bool>.Failed(message, statusCode);
                }

                try
                {
                    // 1. Hoàn trả số lượng vào kho + chuẩn bị movement theo lô
                    var cancelMovementDtos = new List<InventoryMovementDto>();
                    foreach (var item in order.OrderItems)
                    {
                        if (item.Quantity <= 0) return await FailAsync("Invalid order item quantity detected.", 409);

                        await _unitOfWork.ProductVariant.IncreaseStockAsync(item.VariantId, item.Quantity);

                        cancelMovementDtos.Add(new InventoryMovementDto
                        {
                            VariantId = item.VariantId,
                            MovementType = WebBanHang.Model.Enums.InventoryMovementType.IN.ToString(),
                            Quantity = item.Quantity,
                            ReferenceType = "order_cancel",
                            ReferenceId = order.OrderId,
                            Note = $"Restock from cancelled order {order.OrderCode}",
                            CreatedBy = currentUserId ?? order.UserId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    if (cancelMovementDtos.Count > 0)
                    {
                        await _inventoryMovementService.AddRangeAsync(cancelMovementDtos);
                    }

                    // 2. XÓA VẬT LÝ ĐƠN HÀNG 
                    _unitOfWork.Order.Remove(order);
                    
                    await _unitOfWork.SaveAsync();
                    await transaction.CommitAsync();

                    return ApiResponse<bool>.Succeeded(true, "Order has been cancelled and deleted from system.");
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<bool>.Failed("Internal server error while cancelling order.", 500);
                }
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(long id)
        {
            // Admin delete phải đi qua flow huỷ cứng có hoàn kho + movement để tránh lệch tồn.
            return await CancelOrderAsync(id, null);
        }

        private async Task<string?> GenerateUniqueOrderCodeAsync()
        {
            for (var i = 0; i < 5; i++)
            {
                var code = $"ORD-{DateTime.UtcNow:yyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..4].ToUpperInvariant()}";
                var existing = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderCode == code);
                if (existing == null) return code;
            }

            return null;
        }

    }
}
