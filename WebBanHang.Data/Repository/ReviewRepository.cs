using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository {
    public class ReviewRepository : Repository<Review>, IReviewRepository {
        public ReviewRepository(AppDbContext context) : base(context) { }
    }
}
