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
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
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
                        if (existingAddr == null) throw new Exception("Invalid shipping address.");
                        finalAddressId = existingAddr.AddressId;
                    }
                    else
                    {
                        throw new Exception("Please provide a shipping address.");
                    }

                    // 2. Tạo Order mới
                    var order = new Order
                    {
                        UserId = userId,
                        ShippingAddressId = finalAddressId,
                        OrderCode = "ORD-" + DateTime.Now.ToString("yyMMdd") + new Random().Next(1000, 9999),
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

                        if (variant == null) throw new Exception($"Sản phẩm với ID {itemDto.VariantId} không tồn tại.");
                        if (variant.StockQuantity < itemDto.Quantity) throw new Exception($"Sản phẩm {variant.Product.ProductName} đã hết hàng hoặc không đủ số lượng.");

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

                        // Cập nhật tồn kho (Trừ kho)
                        variant.StockQuantity -= itemDto.Quantity;
                        _unitOfWork.ProductVariant.Update(variant);

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
                    order.TotalAmount = subtotal + order.ShippingFee;

                    // Lưu Order vào CSDL
                    await _unitOfWork.Order.AddAsync(order);
                    await _unitOfWork.SaveAsync(); 

                    // Lưu nhật ký kho qua InventoryMovementService
                    foreach (var mDto in movementDtos)
                    {
                        mDto.ReferenceId = order.OrderId;
                        await _inventoryMovementService.AddAsync(mDto);
                    }

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
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<OrderDto>.Failed(ex.Message);
                }
            }
        }

        public async Task<ApiResponse<OrderDto>> AdminUpdateOrderAsync(long id, OrderUpdateDto updateDto)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == id, includeProperties: "OrderItems");
            if (order == null) return ApiResponse<OrderDto>.Failed("Order not found", 404);

            if (!string.IsNullOrEmpty(updateDto.OrderCode)) order.OrderCode = updateDto.OrderCode;
            if (!string.IsNullOrEmpty(updateDto.OrderStatus)) order.OrderStatus = updateDto.OrderStatus;
            if (!string.IsNullOrEmpty(updateDto.PaymentStatus)) order.PaymentStatus = updateDto.PaymentStatus;
            
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
                order.ShippingAddressId = updateDto.ShippingAddressId.Value;
            }

            if (updateDto.ShippingFee.HasValue) order.ShippingFee = updateDto.ShippingFee.Value;
            if (updateDto.SubtotalAmount.HasValue) order.SubtotalAmount = updateDto.SubtotalAmount.Value;

            order.UpdatedAt = DateTime.UtcNow;
            order.TotalAmount = order.SubtotalAmount + order.ShippingFee;

            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();

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
                try
                {
                    // 1. Hoàn trả số lượng vào kho
                    foreach (var item in order.OrderItems)
                    {
                        var variant = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(v => v.VariantId == item.VariantId);
                        if (variant != null)
                        {
                            variant.StockQuantity += item.Quantity;
                            _unitOfWork.ProductVariant.Update(variant);

                            await _inventoryMovementService.AddAsync(new InventoryMovementDto
                            {
                                VariantId = item.VariantId,
                                MovementType = WebBanHang.Model.Enums.InventoryMovementType.IN.ToString(),
                                Quantity = item.Quantity,
                                ReferenceType = "order_cancel",
                                Note = $"Restock from cancelled order {order.OrderCode}",
                                CreatedBy = currentUserId ?? order.UserId,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    // 2. XÓA VẬT LÝ ĐƠN HÀNG (Yêu cầu khách hàng: Hủy là xóa sạch)
                    _unitOfWork.Order.Remove(order);
                    
                    await _unitOfWork.SaveAsync();
                    await transaction.CommitAsync();

                    return ApiResponse<bool>.Succeeded(true, "Order has been cancelled and deleted from system.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<bool>.Failed(ex.Message);
                }
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(long id)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return ApiResponse<bool>.Failed("Order not found", 404);
            _unitOfWork.Order.Remove(order);
            await _unitOfWork.SaveAsync();
            return ApiResponse<bool>.Succeeded(true, "Order deleted permanently.");
        }

        public Task<OrderDto?> CreateOrderFromCart(long cartId)
        {
            throw new NotImplementedException();
        }
    }
}
