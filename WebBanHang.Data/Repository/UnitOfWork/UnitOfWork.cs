using System.Threading.Tasks;
using WebBanHang.Data;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IAddressRepository Address { get; private set; }
        public IBrandRepository Brand { get; private set; }
        public ICartRepository Cart { get; private set; }
        public ICartItemRepository CartItem { get; private set; }
        public ICategoryRepository Category { get; private set; }
        public IColorRepository Color { get; private set; }
        public IInventoryMovementRepository InventoryMovement { get; private set; }
        public IOrderRepository Order { get; private set; }
        public IOrderItemRepository OrderItem { get; private set; }
        public IPaymentRepository Payment { get; private set; }
        public IProductRepository Product { get; private set; }
        public IProductVariantRepository ProductVariant { get; private set; }
        public IReviewRepository Review { get; private set; }
        public IRoleRepository Role { get; private set; }
        public ISizeRepository Size { get; private set; }
        public IUserRepository User { get; private set; }
        public IUserRoleRepository UserRole { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Address = new AddressRepository(_context);
            Brand = new BrandRepository(_context);
            Cart = new CartRepository(_context);
            CartItem = new CartItemRepository(_context);
            Category = new CategoryRepository(_context);
            Color = new ColorRepository(_context);
            InventoryMovement = new InventoryMovementRepository(_context);
            Order = new OrderRepository(_context);
            OrderItem = new OrderItemRepository(_context);
            Payment = new PaymentRepository(_context);
            Product = new ProductRepository(_context);
            ProductVariant = new ProductVariantRepository(_context);
            Review = new ReviewRepository(_context);
            Role = new RoleRepository(_context);
            Size = new SizeRepository(_context);
            User = new UserRepository(_context);
            UserRole = new UserRoleRepository(_context);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
