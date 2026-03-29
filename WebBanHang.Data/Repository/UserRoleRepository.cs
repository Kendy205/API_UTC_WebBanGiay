using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository {
        public UserRoleRepository(AppDbContext context) : base(context) { }
    }
}
