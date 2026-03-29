using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class ColorRepository : Repository<Color>, IColorRepository {
        public ColorRepository(AppDbContext context) : base(context) { }
    }
}
