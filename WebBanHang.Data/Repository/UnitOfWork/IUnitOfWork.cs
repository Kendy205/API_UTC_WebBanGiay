using System;
using System.Threading.Tasks;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IAddressRepository Address { get; }
        IBrandRepository Brand { get; }
        ICartRepository Cart { get; }
        ICartItemRepository CartItem { get; }
        ICategoryRepository Category { get; }
        IColorRepository Color { get; }
        IInventoryMovementRepository InventoryMovement { get; }
        IOrderRepository Order { get; }
        IOrderItemRepository OrderItem { get; }
        IPaymentRepository Payment { get; }
        IProductRepository Product { get; }
        IProductVariantRepository ProductVariant { get; }
        IReviewRepository Review { get; }
        IRoleRepository Role { get; }
        ISizeRepository Size { get; }
        IUserRepository User { get; }
        IUserRoleRepository UserRole { get; }

        Task<int> SaveAsync();
    }
}
