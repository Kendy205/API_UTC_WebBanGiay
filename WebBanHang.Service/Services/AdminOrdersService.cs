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
    public class AdminOrdersService : IAdminOrdersService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminOrdersService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ApiResponse<AdminOrderListResponseDto>> GetOrdersAsync(AdminOrderQueryDto queryDto)
        {
            var entities = await _unitOfWork.Order.GetAllAsync(includeProperties: "User,OrderItems,Payments");
            var query = entities.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(queryDto.Status)) 
                query = query.Where(x => x.OrderStatus.Equals(queryDto.Status, StringComparison.OrdinalIgnoreCase));
            
            if (!string.IsNullOrWhiteSpace(queryDto.Search)) 
                query = query.Where(x => x.OrderCode.Contains(queryDto.Search, StringComparison.OrdinalIgnoreCase) || 
                                        (x.User != null && x.User.FullName.Contains(queryDto.Search, StringComparison.OrdinalIgnoreCase)));

            if (queryDto.StartDate.HasValue) query = query.Where(x => x.CreatedAt >= queryDto.StartDate.Value);
            if (queryDto.EndDate.HasValue) query = query.Where(x => x.CreatedAt <= queryDto.EndDate.Value);

            var total = query.Count();
            var page = queryDto.Page > 0 ? queryDto.Page : 1;
            var pageSize = queryDto.PageSize > 0 ? queryDto.PageSize : 10;
            
            var items = query.OrderByDescending(x => x.CreatedAt)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .Select(o => new AdminOrderListItemDto { 
                                 Id = o.OrderId, 
                                 CustomerName = o.User?.FullName ?? "N/A", 
                                 Total = o.TotalAmount, 
                                 Status = o.OrderStatus, 
                                 CreatedAt = o.CreatedAt,
                                 ItemCount = o.OrderItems.Count,
                                 PaymentMethod = NormalizePaymentMethod(o.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.PaymentMethod)
                             }).ToList();

            return ApiResponse<AdminOrderListResponseDto>.Succeeded(
                new AdminOrderListResponseDto { Data = items, Total = total, Page = page, PageSize = pageSize },
                "Lấy danh sách đơn hàng thành công");
        }

        public async Task<ApiResponse<AdminOrderDetailDto?>> GetOrderDetailAsync(long id)
        {
            var o = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == id, includeProperties: "User,OrderItems.ProductVariant,ShippingAddress,Payments");
            if (o == null) return ApiResponse<AdminOrderDetailDto?>.Failed("Không tìm thấy.", 404);
            
            return ApiResponse<AdminOrderDetailDto?>.Succeeded(new AdminOrderDetailDto {
                Id = o.OrderId, 
                CustomerName = o.User?.FullName ?? "N/A", 
                Address = FormatAddress(o.ShippingAddress),
                Total = o.TotalAmount, 
                Status = o.OrderStatus, 
                PaymentMethod = NormalizePaymentMethod(o.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.PaymentMethod),
                Items = o.OrderItems.Select(i => new AdminOrderDetailItemDto { 
                    ProductId = i.ProductVariant?.ProductId ?? 0,
                    ProductName = i.ProductNameSnapshot, 
                    Quantity = i.Quantity, 
                    UnitPrice = i.UnitPrice 
                }).ToList() 
            }, "Lấy chi tiết đơn hàng thành công");
        }

        public async Task<ApiResponse<AdminOrderStatusResultDto>> UpdateOrderStatusAsync(long id, string status)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == id, includeProperties: "OrderItems");
            if (order == null) return ApiResponse<AdminOrderStatusResultDto>.Failed("Không tìm thấy.", 404);

            if (status.Equals(OrderStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (order.OrderStatus.Equals(OrderStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return ApiResponse<AdminOrderStatusResultDto>.Failed("Đơn hàng đã hủy trước đó.", 400);
                }

                foreach (var item in order.OrderItems) await _unitOfWork.ProductVariant.IncreaseStockAsync(item.VariantId, item.Quantity);
                order.OrderStatus = OrderStatus.Cancelled.ToString();
            }
            else
            {
                order.OrderStatus = status;
            }

            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();
            return ApiResponse<AdminOrderStatusResultDto>.Succeeded(
                new AdminOrderStatusResultDto { Id = id, Status = order.OrderStatus },
                "Cập nhật trạng thái đơn hàng thành công");
        }

        public async Task<ApiResponse<OrderDto>> UpdateOrderAsync(long orderId, OrderUpdateDto updateDto)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return ApiResponse<OrderDto>.Failed("Không thấy.", 404);
            
            if (!string.IsNullOrEmpty(updateDto.OrderStatus)) order.OrderStatus = updateDto.OrderStatus;
            
            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();
            return ApiResponse<OrderDto>.Succeeded(_mapper.Map<OrderDto>(order), "Cập nhật thông tin đơn hàng thành công");
        }

        public async Task<ApiResponse<bool>> DeleteOrderAsync(long orderId)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return ApiResponse<bool>.Failed("Không thấy.", 404);
            
            _unitOfWork.Order.Remove(order);
            await _unitOfWork.SaveAsync();
            return ApiResponse<bool>.Succeeded(true, "Xóa đơn hàng thành công");
        }

        public async Task<ApiResponse<OrderDto>> CreateOrderAsync(CheckoutDto checkoutDto, long customerId)
        {
            if (checkoutDto.Items == null || !checkoutDto.Items.Any())
            {
                return ApiResponse<OrderDto>.Failed("Danh sách sản phẩm đặt hàng không hợp lệ.", 400);
            }

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    long finalAddressId = 0;
                    if (checkoutDto.NewAddress != null)
                    {
                        checkoutDto.NewAddress.UserId = customerId;
                        await _unitOfWork.Address.AddAsync(_mapper.Map<Address>(checkoutDto.NewAddress));
                        await _unitOfWork.SaveAsync();
                        var addr = (await _unitOfWork.Address.GetAllAsync(a => a.UserId == customerId)).OrderByDescending(a => a.AddressId).First();
                        finalAddressId = addr.AddressId;
                    }
                    else if (checkoutDto.ShippingAddressId.HasValue) finalAddressId = checkoutDto.ShippingAddressId.Value;
                    else return ApiResponse<OrderDto>.Failed("Thiếu địa chỉ.", 400);

                    if (checkoutDto.ShippingAddressId.HasValue)
                    {
                        var address = await _unitOfWork.Address.GetFirstOrDefaultAsync(a => a.AddressId == finalAddressId && a.UserId == customerId);
                        if (address == null) return ApiResponse<OrderDto>.Failed("Địa chỉ giao hàng không hợp lệ.", 400);
                    }

                    var order = new Order
                    {
                        UserId = customerId,
                        ShippingAddressId = finalAddressId,
                        OrderCode = $"ORD-ADM-{DateTime.UtcNow:yyMMddHHmmssfff}",
                        OrderStatus = OrderStatus.Pending.ToString(),
                        PaymentStatus = PaymentStatus.Unpaid.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ShippingFee = 30000
                    };

                    decimal subtotal = 0;
                    foreach (var item in checkoutDto.Items)
                    {
                        var variant = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(v => v.VariantId == item.VariantId, includeProperties: "Product");
                        if (variant == null || !await _unitOfWork.ProductVariant.TryDecreaseStockAsync(variant.VariantId, item.Quantity))
                            return ApiResponse<OrderDto>.Failed("Sản phẩm không đủ hàng.", 409);

                        decimal price = variant.PriceOverride ?? variant.Product.SalePrice ?? variant.Product.BasePrice;
                        order.OrderItems.Add(new OrderItem { VariantId = variant.VariantId, ProductNameSnapshot = variant.Product.ProductName, UnitPrice = price, Quantity = item.Quantity, LineTotal = price * item.Quantity });
                        subtotal += price * item.Quantity;
                    }

                    order.SubtotalAmount = subtotal;
                    order.TotalAmount = subtotal + order.ShippingFee;

                    await _unitOfWork.Order.AddAsync(order);
                    await _unitOfWork.SaveAsync();
                    await transaction.CommitAsync();
                    return ApiResponse<OrderDto>.Succeeded(_mapper.Map<OrderDto>(order), "Tạo đơn hàng mới thành công", 201);
                }
                catch { await transaction.RollbackAsync(); throw; }
            }
        }

        private static string? NormalizePaymentMethod(string? method)
        {
            return method?.ToLower() switch
            {
                "cod" => "COD",
                "bank_transfer" => "Banking",
                "banking" => "Banking",
                "vnpay" => "Banking",
                _ => method
            };
        }

        private static string FormatAddress(Address? address)
        {
            if (address == null) return string.Empty;
            return string.Join(", ", new[] { address.StreetAddress, address.Ward, address.District, address.Province }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
