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

        public async Task<ApiResponse<IEnumerable<OrderDto>>> GetMyOrdersAsync(long currentUserId)
        {
            var entities = await _unitOfWork.Order.GetAllAsync(x => x.UserId == currentUserId, includeProperties: "OrderItems,ShippingAddress,OrderItems.ProductVariant.Product,OrderItems.ProductVariant.Size,OrderItems.ProductVariant.Color");
            return ApiResponse<IEnumerable<OrderDto>>.Succeeded(_mapper.Map<IEnumerable<OrderDto>>(entities), "Lấy danh sách đơn hàng thành công");
        }

        public async Task<ApiResponse<OrderDto?>> GetMyOrderByIdAsync(long orderId, long currentUserId)
        {
            var entity = await _unitOfWork.Order.GetFirstOrDefaultAsync(
                x => x.OrderId == orderId && x.UserId == currentUserId, 
                includeProperties: "OrderItems,ShippingAddress,OrderItems.ProductVariant.Product,OrderItems.ProductVariant.Size,OrderItems.ProductVariant.Color");

            if (entity == null) return ApiResponse<OrderDto?>.Failed("Đơn hàng không tồn tại.", 404);
            return ApiResponse<OrderDto?>.Succeeded(_mapper.Map<OrderDto>(entity), "Lấy chi tiết đơn hàng thành công");
        }

        public async Task<ApiResponse<OrderDto>> CheckoutAsync(CheckoutDto checkoutDto, long currentUserId)
        {
            if (checkoutDto.Items == null || !checkoutDto.Items.Any())
            {
                return ApiResponse<OrderDto>.Failed("Danh sách sản phẩm đặt hàng không hợp lệ.", 400);
            }

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var activeCart = await _unitOfWork.Cart.GetFirstOrDefaultAsync(x => x.UserId == currentUserId && x.Status == "active", includeProperties: "CartItems");
                    if (activeCart == null || activeCart.CartItems == null || !activeCart.CartItems.Any()) 
                        return ApiResponse<OrderDto>.Failed("Giỏ hàng trống.", 400);

                    long finalAddressId = 0;
                    if (checkoutDto.NewAddress != null)
                    {
                        checkoutDto.NewAddress.UserId = currentUserId;
                        await _addressService.AddAsync(checkoutDto.NewAddress);
                        var addresses = await _unitOfWork.Address.GetAllAsync(a => a.UserId == currentUserId);
                        finalAddressId = addresses.OrderByDescending(a => a.AddressId).First().AddressId;
                    }
                    else if (checkoutDto.ShippingAddressId.HasValue) finalAddressId = checkoutDto.ShippingAddressId.Value;
                    else return ApiResponse<OrderDto>.Failed("Thiếu địa chỉ.", 400);

                    if (checkoutDto.ShippingAddressId.HasValue)
                    {
                        var address = await _unitOfWork.Address.GetFirstOrDefaultAsync(a => a.AddressId == finalAddressId && a.UserId == currentUserId);
                        if (address == null) return ApiResponse<OrderDto>.Failed("Địa chỉ giao hàng không hợp lệ.", 400);
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
                        ShippingFee = 30000
                    };

                    decimal subtotal = 0;
                    var requestedIds = checkoutDto.Items.Select(item => item.VariantId).ToList();

                    foreach (var itemDto in checkoutDto.Items)
                    {
                        var variant = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(v => v.VariantId == itemDto.VariantId, includeProperties: "Product");
                        if (variant == null) return ApiResponse<OrderDto>.Failed("Sản phẩm không tồn tại.", 404);
                        
                        var decreased = await _unitOfWork.ProductVariant.TryDecreaseStockAsync(variant.VariantId, itemDto.Quantity);
                        if (!decreased) return ApiResponse<OrderDto>.Failed($"Sản phẩm {variant.Product.ProductName} hết hàng.", 409);

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
                    activeCart.Status = activeCart.CartItems.Count == purchasedCartItems.Count ? "converted" : "active";
                    activeCart.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Cart.Update(activeCart);
                    await _unitOfWork.SaveAsync();

                    await transaction.CommitAsync();
                    return ApiResponse<OrderDto>.Succeeded(_mapper.Map<OrderDto>(order), "Đặt hàng thành công", 201);
                }
                catch (Exception) { await transaction.RollbackAsync(); throw; }
            }
        }

        public async Task<ApiResponse<bool>> CancelMyOrderAsync(long orderId, long currentUserId)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == currentUserId, includeProperties: "OrderItems");
            if (order == null) return ApiResponse<bool>.Failed("Không thấy đơn hàng.", 404);

            if (order.OrderStatus == OrderStatus.Cancelled.ToString()) return ApiResponse<bool>.Failed("Đơn hàng đã hủy trước đó.", 400);

            foreach (var item in order.OrderItems) 
            {
                await _unitOfWork.ProductVariant.IncreaseStockAsync(item.VariantId, item.Quantity);
            }
            
            order.OrderStatus = OrderStatus.Cancelled.ToString();
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();
            return ApiResponse<bool>.Succeeded(true, "Hủy đơn hàng thành công");
        }
    }
}
