using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class PaymentRepository : Repository<Payment>, IPaymentRepository {
        public PaymentRepository(AppDbContext context) : base(context) { }
    }
}
