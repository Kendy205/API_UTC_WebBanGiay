using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class CartRepository : Repository<Cart>, ICartRepository {
        public CartRepository(AppDbContext context) : base(context) { }
    }
}
