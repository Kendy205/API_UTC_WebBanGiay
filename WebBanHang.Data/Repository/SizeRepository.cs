using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class SizeRepository : Repository<Size>, ISizeRepository {
        public SizeRepository(AppDbContext context) : base(context) { }
    }
}
