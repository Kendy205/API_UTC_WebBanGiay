using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class CategoryRepository : Repository<Category>, ICategoryRepository {
        public CategoryRepository(AppDbContext context) : base(context) { }
    }
}
