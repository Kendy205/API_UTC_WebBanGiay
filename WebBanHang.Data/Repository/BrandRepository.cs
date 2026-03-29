using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class BrandRepository : Repository<Brand>, IBrandRepository {
        public BrandRepository(AppDbContext context) : base(context) { }
    }
}
