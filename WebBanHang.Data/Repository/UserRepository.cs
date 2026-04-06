using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class UserRepository : Repository<User>, IUserRepository {
        public UserRepository(AppDbContext context) : base(context) { }
    }
}
