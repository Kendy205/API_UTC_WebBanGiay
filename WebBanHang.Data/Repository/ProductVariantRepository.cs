using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class ProductVariantRepository : Repository<ProductVariant>, IProductVariantRepository {
        public ProductVariantRepository(AppDbContext context) : base(context) { }
    }
}
