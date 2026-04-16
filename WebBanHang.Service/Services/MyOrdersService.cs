using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Model.Enums;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Common;
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

        public MyOrdersService(IUnitOfWork unitOfWork, IMapper mapper, IAddressService addressService, IInventoryMovementService inventoryMovementService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _addressService = addressService;
            _inventoryMovementService = inventoryMovementService;
        }

        public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(long currentUserId)
        {
            var entities = await _unitOfWork.Order.GetAllAsync(x => x.UserId == currentUserId, includeProperties: "OrderItems,ShippingAddress,OrderItems.ProductVariant.Product,OrderItems.ProductVariant.Size,OrderItems.ProductVariant.Color");
            return _mapper.Map<IEnumerable<OrderDto>>(entities);
        }

        public async Task<OrderDto?> GetMyOrderByIdAsync(long orderId, long currentUserId)
        {
            var entity = await _unitOfWork.Order.GetFirstOrDefaultAsync(
                x => x.OrderId == orderId && x.UserId == currentUserId,
                includeProperties: "OrderItems,ShippingAddress,OrderItems.ProductVariant.Product,OrderItems.ProductVariant.Size,OrderItems.ProductVariant.Color");

            if (entity == null) return null;
            return _mapper.Map<OrderDto>(entity);
        }

        public async Task<OrderDto> CheckoutAsync(CheckoutDto checkoutDto, long currentUserId)
        {
            if (checkoutDto.Items == null || !checkoutDto.Items.Any())
            {
                throw new Exception("Danh sách sản phẩm đặt hàng không hợp lệ.");
            }

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var activeCart = await _unitOfWork.Cart.GetFirstOrDefaultAsync(x => x.UserId == currentUserId && x.Status == "active", includeProperties: "CartItems");
                    if (activeCart == null || activeCart.CartItems == null || !activeCart.CartItems.Any())
                        throw new Exception("Giỏ hàng trống or đang bảo trì");

                    long finalAddressId = 0;
                    if (checkoutDto.NewAddress != null)
                    {
                        checkoutDto.NewAddress.UserId = currentUserId;
                        await _addressService.AddAsync(checkoutDto.NewAddress);
                        var addresses = await _unitOfWork.Address.GetAllAsync(a => a.UserId == currentUserId);
                        finalAddressId = addresses.OrderByDescending(a => a.AddressId).First().AddressId;
                    }
                    else if (checkoutDto.ShippingAddressId.HasValue) finalAddressId = checkoutDto.ShippingAddressId.Value;
                    else throw new Exception("Thiếu địa chỉ.");

                    if (checkoutDto.ShippingAddressId.HasValue)
                    {
                        var address = await _unitOfWork.Address.GetFirstOrDefaultAsync(a => a.AddressId == finalAddressId && a.UserId == currentUserId);
                        if (address == null) throw new Exception("Địa chỉ giao hàng không hợp lệ.");
                    }

                    var order = new Order
                    {
                        UserId = currentUserId,
                        ShippingAddressId = finalAddressId,
                        OrderCode = $"ORD-{DateTime.UtcNow:yyMMddHHmmssfff}",
                        OrderStatus = OrderStatus.Pending.ToString(),
                        PaymentStatus = PaymentStatus.Unpaid.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ShippingFee = checkoutDto.ShippingFee ?? 30000
                    };

                    decimal subtotal = 0;
                    var requestedIds = checkoutDto.Items.Select(item => item.VariantId).ToList();
                    var movements = new List<InventoryMovementDto>();

                    foreach (var itemDto in checkoutDto.Items)
                    {
                        var variant = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(v => v.VariantId == itemDto.VariantId, includeProperties: "Product");
                        if (variant == null) throw new Exception("Sản phẩm không tồn tại.");
                        
                        var decreased = await _unitOfWork.ProductVariant.TryDecreaseStockAsync(variant.VariantId, itemDto.Quantity);
                        if (!decreased) throw new Exception($"Sản phẩm {variant.Product.ProductName} hết hàng.");

                        decimal price = variant.PriceOverride ?? variant.Product.SalePrice ?? variant.Product.BasePrice;
                        order.OrderItems.Add(new OrderItem { 
                            VariantId = variant.VariantId, 
                            ProductNameSnapshot = variant.Product.ProductName, 
                            UnitPrice = price, 
                            Quantity = itemDto.Quantity, 
                            LineTotal = price * itemDto.Quantity 
                        });
                        subtotal += price * itemDto.Quantity;

                        movements.Add(new InventoryMovementDto
                        {
                            VariantId = variant.VariantId,
                            MovementType = InventoryMovementType.OUT.ToString(),
                            Quantity = itemDto.Quantity,
                            ReferenceType = "order",
                            Note = $"Order checkout: {order.OrderCode}",
                            CreatedBy = currentUserId
                        });
                    }

                    order.SubtotalAmount = subtotal;
                    order.TotalAmount = subtotal + order.ShippingFee;

                    await _unitOfWork.Order.AddAsync(order);
                    await _unitOfWork.SaveAsync();

                    // Cập nhật ReferenceId cho các movement
                    foreach (var m in movements)
                    {
                        m.ReferenceId = order.OrderId;
                    }
                    await _inventoryMovementService.AddRangeAsync(movements);

                    // CẬP NHẬT GIỎ HÀNG: Xóa các item đã mua
                    var purchasedCartItems = activeCart.CartItems.Where(ci => requestedIds.Contains(ci.VariantId)).ToList();
                    foreach (var pi in purchasedCartItems)
                    {
                        _unitOfWork.CartItem.Remove(pi);
                    }
                    // Project only uses "active" cart status currently.
                    activeCart.Status = "active";
                    activeCart.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Cart.Update(activeCart);
                    await _unitOfWork.SaveAsync();

                    await transaction.CommitAsync();
                    return _mapper.Map<OrderDto>(order);
                }
                catch (Exception) { await transaction.RollbackAsync(); throw; }
            }
        }

        public async Task<bool> CancelMyOrderAsync(long orderId, long currentUserId)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == currentUserId, includeProperties: "OrderItems");
                    if (order == null) throw new Exception("Không tìm thấy đơn hàng.");

                    if (order.OrderStatus == OrderStatus.Cancelled.ToString()) throw new Exception("Đơn hàng đã hủy trước đó.");

                    var movements = new List<InventoryMovementDto>();
                    foreach (var item in order.OrderItems) 
                    {
                        await _unitOfWork.ProductVariant.IncreaseStockAsync(item.VariantId, item.Quantity);
                        movements.Add(new InventoryMovementDto
                        {
                            VariantId = item.VariantId,
                            MovementType = InventoryMovementType.IN.ToString(),
                            Quantity = item.Quantity,
                            ReferenceType = "order_cancel",
                            ReferenceId = order.OrderId,
                            Note = $"Order cancelled by user: {order.OrderCode}",
                            CreatedBy = currentUserId
                        });
                    }
                    
                    await _inventoryMovementService.AddRangeAsync(movements);

                    order.OrderStatus = OrderStatus.Cancelled.ToString();
                    order.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Order.Update(order);
                    await _unitOfWork.SaveAsync();

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception) { await transaction.RollbackAsync(); throw; }
            }
        }
    }
}
