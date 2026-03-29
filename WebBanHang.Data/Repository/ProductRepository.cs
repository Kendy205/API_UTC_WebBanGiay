using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class ProductRepository : Repository<Product>, IProductRepository {
        public ProductRepository(AppDbContext context) : base(context) { }
    }
}
