using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class OrderRepository : Repository<Order>, IOrderRepository {
        public OrderRepository(AppDbContext context) : base(context) { }
    }
}
