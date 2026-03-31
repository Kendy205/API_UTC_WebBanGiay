using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class InventoryMovementRepository : Repository<InventoryMovement>, IInventoryMovementRepository {
        public InventoryMovementRepository(AppDbContext context) : base(context) { }
    }
}
