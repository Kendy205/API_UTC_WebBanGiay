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
using WebBanHang.Service.Exceptions;
using WebBanHang.Model.Enums;

namespace WebBanHang.Service.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAddressService _addressService;
        private readonly IInventoryMovementService _inventoryMovementService;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, IAddressService addressService, IInventoryMovementService inventoryMovementService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _addressService = addressService;
            _inventoryMovementService = inventoryMovementService;
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersAsync(long userId, bool isAdmin)
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
            return dtos;
        }

        public async Task<OrderDto> GetByIdAsync(long id, long userId, bool isAdmin)
        {
            var entity = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == id, includeProperties: "User,OrderItems,ShippingAddress");
            if (entity == null)
                throw new ApiException("Không tìm thấy đơn hàng.", 404);
            
            if (!isAdmin && entity.UserId != userId)
                throw new ApiException("Bạn không có quyền xem đơn hàng này.", 403);

            var dto = _mapper.Map<OrderDto>(entity);
            return dto;
        }

        public async Task<AdminOrderListResponseDto> GetAdminOrdersAsync(AdminOrderQueryDto queryDto)
        {
            var page = queryDto.Page <= 0 ? 1 : queryDto.Page;
            var pageSize = queryDto.PageSize <= 0 ? 10 : queryDto.PageSize;

            var entities = await _unitOfWork.Order.GetAllAsync(includeProperties: "User,OrderItems,Payments");
            IEnumerable<Order> query = entities;

            if (!string.IsNullOrWhiteSpace(queryDto.Status))
            {
                if (!TryNormalizeAdminStatus(queryDto.Status, out var normalizedStatus))
                {
                    throw new ApiException("Trạng thái đơn hàng không hợp lệ.", 400);
                }

                query = query.Where(x => string.Equals(x.OrderStatus, normalizedStatus, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(queryDto.Search))
            {
                var search = queryDto.Search.Trim();
                query = query.Where(x =>
                    (!string.IsNullOrWhiteSpace(x.OrderCode) && x.OrderCode.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (x.User != null && !string.IsNullOrWhiteSpace(x.User.FullName) && x.User.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            var total = query.Count();
            var items = query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapAdminOrderListItem)
                .ToList();

            var response = new AdminOrderListResponseDto
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };

            return response;
        }

        public async Task<AdminOrderDetailDto> GetAdminOrderDetailAsync(long id)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(
                x => x.OrderId == id,
                includeProperties: "User,OrderItems.ProductVariant,ShippingAddress,Payments");

            if (order == null) throw new ApiException("Không tìm thấy đơn hàng.", 404);

            var detail = new AdminOrderDetailDto
            {
                Id = order.OrderId,
                CustomerName = order.User?.FullName ?? string.Empty,
                Address = BuildAddressLine(order.ShippingAddress),
                Total = order.TotalAmount,
                Status = ToAdminStatusLabel(order.OrderStatus),
                PaymentMethod = NormalizeAdminPaymentMethodLabel(order.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.PaymentMethod),
                Items = order.OrderItems.Select(x => new AdminOrderDetailItemDto
                {
                    ProductId = x.ProductVariant?.ProductId ?? x.VariantId,
                    ProductName = x.ProductNameSnapshot,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice
                }).ToList()
            };

            return detail;
        }

        public async Task<AdminOrderStatusResultDto> AdminUpdateOrderStatusAsync(long id, string status, long adminUserId)
        {
            if (!TryNormalizeAdminUpdatableStatus(status, out var normalizedStatus))
            {
                throw new ApiException("Trạng thái không hợp lệ.", 400);
            }

            if (normalizedStatus == WebBanHang.Model.Enums.OrderStatus.Cancelled.ToString())
            {
                await CancelOrderAsync(id, null);

                return new AdminOrderStatusResultDto
                {
                    Id = id,
                    Status = "Cancelled"
                };
            }

            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(x => x.OrderId == id);
            if (order == null) throw new ApiException("Không tìm thấy đơn hàng.", 404);

            if (string.Equals(order.OrderStatus, WebBanHang.Model.Enums.OrderStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw new ApiException("Đơn hàng đã hủy không thể chuyển sang trạng thái khác.", 400);
            }

            order.OrderStatus = normalizedStatus;
            order.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Order.Update(order);
            await _unitOfWork.SaveAsync();

            return new AdminOrderStatusResultDto
            {
                Id = id,
                Status = ToAdminStatusLabel(order.OrderStatus)
            };
        }

        public async Task<OrderDto> PlaceOrderAsync(CheckoutDto checkoutDto, long userId)
        {
            if (userId <= 0)
            {
                throw new ApiException("Vui lòng đăng nhập.", 401);
            }

            if (checkoutDto.Items == null || !checkoutDto.Items.Any())
            {
                throw new ApiException("Danh sách sản phẩm không được để trống.", 400);
            }

            if (checkoutDto.Items.Any(x => x.Quantity <= 0))
            {
                throw new ApiException("Số lượng phải lớn hơn 0.", 400);
            }

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                async Task FailAsync(string message, int statusCode = 400)
                {
                    await transaction.RollbackAsync();
                    throw new ApiException(message, statusCode);
                }

                try
                {
                    var activeCart = await _unitOfWork.Cart.GetFirstOrDefaultAsync(
                        x => x.UserId == userId && x.Status == "active",
                        includeProperties: "CartItems");

                    if (activeCart == null)
                    {
                        await FailAsync("Không tìm thấy giỏ hàng đang hoạt động.", 400);
                        return null!;
                    }

                    if (!activeCart.CartItems.Any())
                    {
                        await FailAsync("Giỏ hàng đang trống.", 400);
                        return null!;
                    }

                    var requestedVariantIds = checkoutDto.Items.Select(x => x.VariantId).ToList();
                    if (requestedVariantIds.Distinct().Count() != requestedVariantIds.Count)
                    {
                        await FailAsync("Không được phép đặt trùng biến thể sản phẩm trong cùng một đơn.", 400);
                        return null!;
                    }

                    var cartItemsByVariant = activeCart.CartItems.ToDictionary(x => x.VariantId);
                    foreach (var itemDto in checkoutDto.Items)
                    {
                        if (!cartItemsByVariant.TryGetValue(itemDto.VariantId, out var cartItem))
                        {
                            await FailAsync($"Biến thể {itemDto.VariantId} không tồn tại trong giỏ hàng.", 400);
                            return null!;
                        }

                        if (cartItem.Quantity != itemDto.Quantity)
                        {
                            await FailAsync($"Số lượng trong giỏ của biến thể {itemDto.VariantId} không khớp. Vui lòng tải lại giỏ hàng trước khi đặt hàng.", 409);
                            return null!;
                        }
                    }

                    // 1. Nếu có địa chỉ mới, dùng luồng AddressService như api/address đang dùng.
                    long finalAddressId = 0;
                    if (checkoutDto.NewAddress != null)
                    {
                        checkoutDto.NewAddress.UserId = userId;
                        await _addressService.AddAsync(checkoutDto.NewAddress);

                        var matchedAddresses = await _unitOfWork.Address.GetAllAsync(a =>
                            a.UserId == userId &&
                            a.RecipientName == checkoutDto.NewAddress.RecipientName &&
                            a.Phone == checkoutDto.NewAddress.Phone &&
                            a.Province == checkoutDto.NewAddress.Province &&
                            a.District == checkoutDto.NewAddress.District &&
                            a.Ward == checkoutDto.NewAddress.Ward &&
                            a.StreetAddress == checkoutDto.NewAddress.StreetAddress);

                        var createdAddress = matchedAddresses
                            .OrderByDescending(a => a.AddressId)
                            .FirstOrDefault();

                        if (createdAddress == null)
                        {
                            await FailAsync("Không thể tạo địa chỉ giao hàng.", 500);
                            return null!;
                        }

                        finalAddressId = createdAddress.AddressId;
                    }
                    else if (checkoutDto.ShippingAddressId.HasValue)
                    {
                        var existingAddr = await _unitOfWork.Address.GetFirstOrDefaultAsync(a => a.AddressId == checkoutDto.ShippingAddressId.Value && a.UserId == userId);
                        if (existingAddr == null)
                        {
                            await FailAsync("Địa chỉ giao hàng không hợp lệ.", 400);
                            return null!;
                        }
                        finalAddressId = existingAddr.AddressId;
                    }
                    else
                    {
                        await FailAsync("Vui lòng cung cấp địa chỉ giao hàng.", 400);
                        return null!;
                    }

                    // 2. Tạo Order mới
                    var orderCode = await GenerateUniqueOrderCodeAsync();
                    if (string.IsNullOrEmpty(orderCode))
                    {
                        await FailAsync("Không thể tạo mã đơn hàng.", 409);
                        return null!;
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

                        if (variant == null)
                        {
                            await FailAsync($"Không tìm thấy biến thể sản phẩm {itemDto.VariantId}.", 404);
                            return null!;
                        }

                        var stockDecreased = await _unitOfWork.ProductVariant.TryDecreaseStockAsync(variant.VariantId, itemDto.Quantity);
                        if (!stockDecreased)
                        {
                            await FailAsync($"Sản phẩm {variant.Product.ProductName} đã hết hàng hoặc không đủ số lượng.", 409);
                            return null!;
                        }

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
                        await FailAsync("Tổng tiền đơn hàng không hợp lệ.", 400);
                        return null!;
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

                    // 4. Chỉ xóa các cart item đã checkout để giữ phần còn lại trong giỏ.
                    var purchasedCartItems = activeCart.CartItems
                        .Where(x => requestedVariantIds.Contains(x.VariantId))
                        .ToList();

                    foreach (var cartItem in purchasedCartItems)
                    {
                        _unitOfWork.CartItem.Remove(cartItem);
                    }

                    activeCart.UpdatedAt = DateTime.UtcNow;
                    activeCart.Status = activeCart.CartItems.Count == purchasedCartItems.Count ? "converted" : "active";
                    _unitOfWork.Cart.Update(activeCart);
                    await _unitOfWork.SaveAsync();

                    await transaction.CommitAsync();

                    return _mapper.Map<OrderDto>(order);
                }
                catch (DbUpdateException)
                {
                    await transaction.RollbackAsync();
                    throw new ApiException("Đơn hàng đang bị xung đột dữ liệu. Vui lòng thử lại.", 409);
                }
                catch (ApiException)
                {
                    throw;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw new ApiException("Có lỗi xảy ra khi đặt hàng.", 500);
                }
            }
        }

        public async Task<OrderDto> AdminUpdateOrderAsync(long id, OrderUpdateDto updateDto)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == id, includeProperties: "OrderItems");
            if (order == null) throw new ApiException("Không tìm thấy đơn hàng.", 404);

            if (!string.IsNullOrEmpty(updateDto.OrderCode)) order.OrderCode = updateDto.OrderCode;
            if (!string.IsNullOrEmpty(updateDto.OrderStatus))
            {
                if (!Enum.TryParse<WebBanHang.Model.Enums.OrderStatus>(updateDto.OrderStatus, true, out var parsedOrderStatus))
                    throw new ApiException("Trạng thái đơn hàng không hợp lệ.", 400);

                order.OrderStatus = parsedOrderStatus.ToString();
            }

            if (!string.IsNullOrEmpty(updateDto.PaymentStatus))
            {
                if (!Enum.TryParse<WebBanHang.Model.Enums.PaymentStatus>(updateDto.PaymentStatus, true, out var parsedPaymentStatus))
                    throw new ApiException("Trạng thái thanh toán không hợp lệ.", 400);

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
                    throw new ApiException("Không tìm thấy địa chỉ giao hàng.", 404);

                order.ShippingAddressId = updateDto.ShippingAddressId.Value;
            }

            if (updateDto.ShippingFee.HasValue)
            {
                if (updateDto.ShippingFee.Value < 0) throw new ApiException("Phí vận chuyển không được nhỏ hơn 0.", 400);
                order.ShippingFee = updateDto.ShippingFee.Value;
            }

            if (updateDto.DiscountAmount.HasValue)
            {
                if (updateDto.DiscountAmount.Value < 0) throw new ApiException("Giảm giá không được nhỏ hơn 0.", 400);
                order.DiscountAmount = updateDto.DiscountAmount.Value;
            }

            order.SubtotalAmount = order.OrderItems.Sum(x => x.LineTotal);
            order.UpdatedAt = DateTime.UtcNow;
            order.TotalAmount = order.SubtotalAmount + order.ShippingFee - order.DiscountAmount;
            if (order.TotalAmount < 0) throw new ApiException("Tổng tiền đơn hàng không được nhỏ hơn 0.", 400);

            _unitOfWork.Order.Update(order);
            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch (DbUpdateException)
            {
                throw new ApiException("Cập nhật đơn hàng bị xung đột dữ liệu.", 409);
            }

            return _mapper.Map<OrderDto>(order);
        }

        public async Task CancelOrderAsync(long orderId, long? currentUserId = null)
        {
            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(o => o.OrderId == orderId, includeProperties: "OrderItems,Payments");
            if (order == null) throw new ApiException("Không tìm thấy đơn hàng.", 404);

            if (currentUserId.HasValue && order.UserId != currentUserId.Value)
                throw new ApiException("Bạn không có quyền thao tác với đơn hàng này.", 403);

            if (string.Equals(order.OrderStatus, WebBanHang.Model.Enums.OrderStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase))
                throw new ApiException("Đơn hàng đã được hủy trước đó.", 400);

            // Chỉ cho phép hủy khi chưa giao xong
            if (order.OrderStatus == WebBanHang.Model.Enums.OrderStatus.Shipped.ToString() || order.OrderStatus == WebBanHang.Model.Enums.OrderStatus.Delivered.ToString())
                throw new ApiException("Không thể hủy đơn hàng đã giao hoặc đã hoàn thành.", 400);

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                async Task FailAsync(string message, int statusCode = 400)
                {
                    await transaction.RollbackAsync();
                    throw new ApiException(message, statusCode);
                }

                try
                {
                    // 1. Hoàn trả số lượng vào kho + chuẩn bị movement theo lô
                    var cancelMovementDtos = new List<InventoryMovementDto>();
                    foreach (var item in order.OrderItems)
                    {
                        if (item.Quantity <= 0)
                        {
                            await FailAsync("Phát hiện số lượng sản phẩm trong đơn không hợp lệ.", 409);
                            return;
                        }

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

                    // 2. Soft cancel: giữ đơn, đổi trạng thái sang Cancelled.
                    order.OrderStatus = WebBanHang.Model.Enums.OrderStatus.Cancelled.ToString();
                    var hasSuccessfulPayment = order.Payments.Any(x =>
                        string.Equals(x.PaymentStatus, WebBanHang.Model.Enums.PaymentStatus.Paid.ToString(), StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(x.PaymentStatus, "Success", StringComparison.OrdinalIgnoreCase));

                    if (order.Payments.Any())
                    {
                        foreach (var payment in order.Payments)
                        {
                            if (string.Equals(payment.PaymentStatus, WebBanHang.Model.Enums.PaymentStatus.Paid.ToString(), StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(payment.PaymentStatus, "Success", StringComparison.OrdinalIgnoreCase))
                            {
                                payment.PaymentStatus = WebBanHang.Model.Enums.PaymentStatus.Refunded.ToString();
                            }
                            else if (string.Equals(payment.PaymentStatus, "Pending", StringComparison.OrdinalIgnoreCase))
                            {
                                payment.PaymentStatus = WebBanHang.Model.Enums.PaymentStatus.Failed.ToString();
                            }

                            _unitOfWork.Payment.Update(payment);
                        }
                    }

                    order.PaymentStatus = hasSuccessfulPayment
                        ? WebBanHang.Model.Enums.PaymentStatus.Refunded.ToString()
                        : WebBanHang.Model.Enums.PaymentStatus.Unpaid.ToString();
                    order.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Order.Update(order);
                    
                    await _unitOfWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (ApiException)
                {
                    throw;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw new ApiException("Có lỗi xảy ra khi hủy đơn hàng.", 500);
                }
            }
        }

        public async Task DeleteAsync(long id, long deletedByUserId)
        {
            if (deletedByUserId <= 0)
            {
                throw new ApiException("Vui lòng đăng nhập.", 401);
            }

            var order = await _unitOfWork.Order.GetFirstOrDefaultAsync(
                o => o.OrderId == id,
                includeProperties: "OrderItems,Payments");

            if (order == null) throw new ApiException("Không tìm thấy đơn hàng.", 404);

            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var shouldRestock = !string.Equals(order.OrderStatus, WebBanHang.Model.Enums.OrderStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(order.OrderStatus, WebBanHang.Model.Enums.OrderStatus.Shipped.ToString(), StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(order.OrderStatus, WebBanHang.Model.Enums.OrderStatus.Delivered.ToString(), StringComparison.OrdinalIgnoreCase);

                    if (shouldRestock && order.OrderItems.Any())
                    {
                        var deleteMovementDtos = new List<InventoryMovementDto>();

                        foreach (var item in order.OrderItems)
                        {
                            if (item.Quantity <= 0)
                            {
                                await transaction.RollbackAsync();
                                throw new ApiException("Phát hiện số lượng sản phẩm trong đơn không hợp lệ.", 409);
                            }

                            await _unitOfWork.ProductVariant.IncreaseStockAsync(item.VariantId, item.Quantity);

                            deleteMovementDtos.Add(new InventoryMovementDto
                            {
                                VariantId = item.VariantId,
                                MovementType = InventoryMovementType.IN.ToString(),
                                Quantity = item.Quantity,
                                ReferenceType = "order_delete",
                                ReferenceId = order.OrderId,
                                Note = $"Restock from hard-deleted order {order.OrderCode}",
                                CreatedBy = deletedByUserId,
                                CreatedAt = DateTime.UtcNow
                            });
                        }

                        await _inventoryMovementService.AddRangeAsync(deleteMovementDtos);
                    }

                    if (order.Payments.Any())
                    {
                        _unitOfWork.Payment.RemoveRange(order.Payments);
                    }

                    if (order.OrderItems.Any())
                    {
                        _unitOfWork.OrderItem.RemoveRange(order.OrderItems);
                    }

                    _unitOfWork.Order.Remove(order);
                    await _unitOfWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (ApiException)
                {
                    throw;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw new ApiException("Có lỗi xảy ra khi xóa đơn hàng.", 500);
                }
            }
        }

        private static AdminOrderListItemDto MapAdminOrderListItem(Order order)
        {
            return new AdminOrderListItemDto
            {
                Id = order.OrderId,
                CustomerName = order.User?.FullName ?? string.Empty,
                Total = order.TotalAmount,
                Status = ToAdminStatusLabel(order.OrderStatus),
                CreatedAt = order.CreatedAt,
                ItemCount = order.OrderItems?.Count ?? 0,
                PaymentMethod = NormalizeAdminPaymentMethodLabel(order.Payments?.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.PaymentMethod)
            };
        }

        private static string BuildAddressLine(Address? address)
        {
            if (address == null) return string.Empty;

            var segments = new[]
            {
                address.StreetAddress,
                address.Ward,
                address.District,
                address.Province
            };

            return string.Join(", ", segments.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private static string ToAdminStatusLabel(string status)
        {
            if (string.Equals(status, WebBanHang.Model.Enums.OrderStatus.Shipped.ToString(), StringComparison.OrdinalIgnoreCase))
                return "Shipping";

            if (string.Equals(status, WebBanHang.Model.Enums.OrderStatus.Delivered.ToString(), StringComparison.OrdinalIgnoreCase))
                return "Completed";

            return status;
        }

        private static bool TryNormalizeAdminStatus(string? rawStatus, out string normalizedStatus)
        {
            normalizedStatus = string.Empty;
            if (string.IsNullOrWhiteSpace(rawStatus)) return false;

            var value = rawStatus.Trim().ToLowerInvariant();
            normalizedStatus = value switch
            {
                "pending" => WebBanHang.Model.Enums.OrderStatus.Pending.ToString(),
                "confirmed" => WebBanHang.Model.Enums.OrderStatus.Confirmed.ToString(),
                "shipping" => WebBanHang.Model.Enums.OrderStatus.Shipped.ToString(),
                "shipped" => WebBanHang.Model.Enums.OrderStatus.Shipped.ToString(),
                "completed" => WebBanHang.Model.Enums.OrderStatus.Delivered.ToString(),
                "delivered" => WebBanHang.Model.Enums.OrderStatus.Delivered.ToString(),
                "cancelled" => WebBanHang.Model.Enums.OrderStatus.Cancelled.ToString(),
                "canceled" => WebBanHang.Model.Enums.OrderStatus.Cancelled.ToString(),
                _ => string.Empty
            };

            return !string.IsNullOrWhiteSpace(normalizedStatus);
        }

        private static bool TryNormalizeAdminUpdatableStatus(string? rawStatus, out string normalizedStatus)
        {
            normalizedStatus = string.Empty;
            if (!TryNormalizeAdminStatus(rawStatus, out var candidate))
            {
                return false;
            }

            if (string.Equals(candidate, WebBanHang.Model.Enums.OrderStatus.Pending.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            normalizedStatus = candidate;
            return true;
        }

        private static string NormalizeAdminPaymentMethodLabel(string? rawMethod)
        {
            var method = rawMethod?.Trim().ToLowerInvariant();
            return method switch
            {
                "cod" => "COD",
                "banking" => "Banking",
                "bank_transfer" => "Banking",
                "banktransfer" => "Banking",
                _ => rawMethod?.Trim() ?? string.Empty
            };
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
