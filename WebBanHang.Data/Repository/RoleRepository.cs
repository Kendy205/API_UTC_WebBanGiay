using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class RoleRepository : Repository<Role>, IRoleRepository {
        public RoleRepository(AppDbContext context) : base(context) { }
    }
}
