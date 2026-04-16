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
        private readonly IInventoryMovementService _inventoryMovementService;

        public AdminOrdersService(IUnitOfWork unitOfWork, IMapper mapper, IInventoryMovementService inventoryMovementService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _inventoryMovementService = inventoryMovementService;
        }

        public async Task<AdminOrderListResponseDto> GetOrdersAsync(AdminOrderQueryDto queryDto)
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

            return new AdminOrderListResponseDto { Data = items, Total = total, Page = page, PageSize = pageSize };
        }

        public async Task<AdminOrderDetailDto?> GetOrderDetailAsync(long id)
        {
            var o = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == id, includeProperties: "User,OrderItems.ProductVariant,ShippingAddress,Payments");
            if (o == null) return null;
            
            return new AdminOrderDetailDto {
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
            };
        }

        public async Task<AdminOrderStatusResultDto> UpdateOrderStatusAsync(long id, string status)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == id, includeProperties: "OrderItems");
                    if (order == null) throw new Exception("Không tìm thấy đơn hàng.");

                    if (status.Equals(OrderStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        if (order.OrderStatus.Equals(OrderStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception("Đơn hàng đã hủy trước đó.");
                        }

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
                                Note = $"Order cancelled by Admin: {order.OrderCode}",
                                CreatedBy = order.UserId // Tạm thời dùng UserId của chủ đơn vì không có context adminId ở đây
                            });
                        }
                        await _inventoryMovementService.AddRangeAsync(movements);
                        order.OrderStatus = OrderStatus.Cancelled.ToString();
                    }
                    else
                    {
                        order.OrderStatus = status;
                    }

                    _unitOfWork.Order.Update(order);
                    await _unitOfWork.SaveAsync();
                    await transaction.CommitAsync();
                    return new AdminOrderStatusResultDto { Id = id, Status = order.OrderStatus };
                }
                catch (Exception) { await transaction.RollbackAsync(); throw; }
            }
        }

        public async Task<OrderDto> UpdateOrderAsync(long orderId, OrderUpdateDto updateDto)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) throw new Exception("Không tìm thấy đơn hàng.");
            
            if (!string.IsNullOrEmpty(updateDto.OrderStatus)) order.OrderStatus = updateDto.OrderStatus;
            
            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<bool> DeleteOrderAsync(long orderId)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) throw new Exception("Không tìm thấy đơn hàng.");
            
            _unitOfWork.Order.Remove(order);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<OrderDto> CreateOrderAsync(CheckoutDto checkoutDto, long customerId)
        {
            if (checkoutDto.Items == null || !checkoutDto.Items.Any())
            {
                throw new Exception("Danh sách sản phẩm đặt hàng không hợp lệ.");
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
                    else throw new Exception("Thiếu địa chỉ.");

                    if (checkoutDto.ShippingAddressId.HasValue)
                    {
                        var address = await _unitOfWork.Address.GetFirstOrDefaultAsync(a => a.AddressId == finalAddressId && a.UserId == customerId);
                        if (address == null) throw new Exception("Địa chỉ giao hàng không hợp lệ.");
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
	                        ShippingFee = checkoutDto.ShippingFee ?? 30000
	                    };

                    decimal subtotal = 0;
                    var movements = new List<InventoryMovementDto>();
                    foreach (var item in checkoutDto.Items)
                    {
                        var variant = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(v => v.VariantId == item.VariantId, includeProperties: "Product");
                        if (variant == null) throw new Exception("Sản phẩm không tồn tại.");
                        if (!await _unitOfWork.ProductVariant.TryDecreaseStockAsync(variant.VariantId, item.Quantity))
                            throw new Exception("Sản phẩm không đủ hàng.");

                        decimal price = variant.PriceOverride ?? variant.Product.SalePrice ?? variant.Product.BasePrice;
                        order.OrderItems.Add(new OrderItem { VariantId = variant.VariantId, ProductNameSnapshot = variant.Product.ProductName, UnitPrice = price, Quantity = item.Quantity, LineTotal = price * item.Quantity });
                        subtotal += price * item.Quantity;

                        movements.Add(new InventoryMovementDto
                        {
                            VariantId = variant.VariantId,
                            MovementType = InventoryMovementType.OUT.ToString(),
                            Quantity = item.Quantity,
                            ReferenceType = "order",
                            Note = $"Admin created order: {order.OrderCode}",
                            CreatedBy = customerId // Tạm thời dùng customerId
                        });
                    }

                    order.SubtotalAmount = subtotal;
                    order.TotalAmount = subtotal + order.ShippingFee;

                    await _unitOfWork.Order.AddAsync(order);
                    await _unitOfWork.SaveAsync();

                    foreach (var m in movements)
                    {
                        m.ReferenceId = order.OrderId;
                    }
                    await _inventoryMovementService.AddRangeAsync(movements);

                    await transaction.CommitAsync();
                    return _mapper.Map<OrderDto>(order);
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
