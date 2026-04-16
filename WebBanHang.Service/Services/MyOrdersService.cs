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

        public MyOrdersService(IUnitOfWork unitOfWork, IMapper mapper, IAddressService addressService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _addressService = addressService;
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
                        throw new Exception("Giỏ hàng trống.");

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
                    }

                    order.SubtotalAmount = subtotal;
                    order.TotalAmount = subtotal + order.ShippingFee;

                    await _unitOfWork.Order.AddAsync(order);
                    await _unitOfWork.SaveAsync();

                    // CẬP NHẬT GIỎ HÀNG: Xóa các item đã mua
                    var purchasedCartItems = activeCart.CartItems.Where(ci => requestedIds.Contains(ci.VariantId)).ToList();
                    foreach (var pi in purchasedCartItems)
                    {
                        _unitOfWork.CartItem.Remove(pi);
                    }
                    // Project currently only uses "active" status for cart.
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
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == currentUserId, includeProperties: "OrderItems");
            if (order == null) throw new Exception("Không tìm thấy đơn hàng.");

            if (order.OrderStatus == OrderStatus.Cancelled.ToString()) throw new Exception("Đơn hàng đã hủy trước đó.");

            foreach (var item in order.OrderItems) 
            {
                await _unitOfWork.ProductVariant.IncreaseStockAsync(item.VariantId, item.Quantity);
            }
            
            order.OrderStatus = OrderStatus.Cancelled.ToString();
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
