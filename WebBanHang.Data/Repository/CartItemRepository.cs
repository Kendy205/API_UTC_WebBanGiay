using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class CartItemRepository : Repository<CartItem>, ICartItemRepository {
        public CartItemRepository(AppDbContext context) : base(context) { }
    }
}
