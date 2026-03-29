using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class AddressRepository : Repository<Address>, IAddressRepository {
        public AddressRepository(AppDbContext context) : base(context) { }
    }
}
