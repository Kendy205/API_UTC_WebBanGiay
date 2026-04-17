using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebBanHang.Model;
using WebBanHang.Model.Enums;
using WebBanHang.Repository.UnitOfWork;
using WebBanHang.Service.DTOs.Model;
using WebBanHang.Service.DTOs.Order;
using WebBanHang.Service.IServices;

namespace WebBanHang.Service.Services
{
    public class InventoryMovementService : IInventoryMovementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InventoryMovementService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ================= TRUY VẤN CƠ BẢN =================

        public async Task<IEnumerable<InventoryMovementDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.InventoryMovement.GetAllAsync();
            return _mapper.Map<IEnumerable<InventoryMovementDto>>(entities);
        }

        public async Task<InventoryMovementDto?> GetByIdAsync(long id)
        {
            var entity = await _unitOfWork.InventoryMovement.GetFirstOrDefaultAsync(x => x.MovementId == id);
            return _mapper.Map<InventoryMovementDto>(entity);
        }

        // ================= NGHIỆP VỤ KHO CHI TIẾT =================

        /// <summary>
        /// Xử lý trừ kho và ghi log khi đặt hàng
        /// </summary>
        public async Task HandleCheckoutAsync(Order order, IEnumerable<CartItemLocalDto> items, long userId)
        {
            foreach (var item in items)
            {
                // 1. Trừ kho nguyên tử (Atomic Update)
                var success = await _unitOfWork.ProductVariant.TryDecreaseStockAsync(item.VariantId, item.Quantity);

                if (!success)
                {
                    // Lấy thông tin sản phẩm để báo lỗi chi tiết
                    var v = await _unitOfWork.ProductVariant.GetFirstOrDefaultAsync(x => x.VariantId == item.VariantId, "Product");
                    throw new Exception($"Sản phẩm {v?.Product?.ProductName} đã hết hàng hoặc không đủ số lượng.");
                }

                // 2. Tạo bản ghi biến động kho (Loại OUT)
                await _unitOfWork.InventoryMovement.AddAsync(new InventoryMovement
                {
                    VariantId = item.VariantId,
                    MovementType = InventoryMovementType.OUT.ToString(),
                    Quantity = item.Quantity,
                    ReferenceType = "order",
                    ReferenceId = order.OrderId,
                    Note = $"Xuất kho cho đơn hàng: {order.OrderCode}",
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }
            // Lưu thay đổi (SaveAsync) sẽ do UnitOfWork ở MyOrdersService thực hiện để đảm bảo Transaction
        }

        /// <summary>
        /// Xử lý hoàn kho khi đơn hàng bị hủy
        /// </summary>
        public async Task HandleOrderCancelAsync(Order order, long userId)
        {
            if (order.OrderItems == null) return;

            foreach (var item in order.OrderItems)
            {
                // 1. Cộng lại kho
                await _unitOfWork.ProductVariant.IncreaseStockAsync(item.VariantId, item.Quantity);

                // 2. Tạo bản ghi biến động kho (Loại IN)
                await _unitOfWork.InventoryMovement.AddAsync(new InventoryMovement
                {
                    VariantId = item.VariantId,
                    MovementType = InventoryMovementType.IN.ToString(),
                    Quantity = item.Quantity,
                    ReferenceType = "order_cancel",
                    ReferenceId = order.OrderId,
                    Note = $"Hoàn kho do hủy đơn: {order.OrderCode}",
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // ================= CÁC HÀM CRUD KHÁC =================

        public async Task AddAsync(InventoryMovementDto dto)
        {
            var entity = _mapper.Map<InventoryMovement>(dto);
            await _unitOfWork.InventoryMovement.AddAsync(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task AddRangeAsync(IEnumerable<InventoryMovementDto> dtos)
        {
            var entities = _mapper.Map<IEnumerable<InventoryMovement>>(dtos);
            foreach (var entity in entities)
            {
                await _unitOfWork.InventoryMovement.AddAsync(entity);
            }
            await _unitOfWork.SaveAsync();
        }

        public async Task UpdateAsync(long id, InventoryMovementDto dto)
        {
            var entity = await _unitOfWork.InventoryMovement.GetFirstOrDefaultAsync(x => x.MovementId == id);
            if (entity != null)
            {
                _mapper.Map(dto, entity);
                _unitOfWork.InventoryMovement.Update(entity);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task DeleteAsync(long id)
        {
            var entity = await _unitOfWork.InventoryMovement.GetFirstOrDefaultAsync(x => x.MovementId == id);
            if (entity != null)
            {
                _unitOfWork.InventoryMovement.Remove(entity);
                await _unitOfWork.SaveAsync();
            }
        }
    }
}