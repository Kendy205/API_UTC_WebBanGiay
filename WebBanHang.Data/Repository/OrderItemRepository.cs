using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository {
        public OrderItemRepository(AppDbContext context) : base(context) { }
    }
}
