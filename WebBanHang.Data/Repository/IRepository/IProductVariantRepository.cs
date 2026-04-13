using WebBanHang.Model;
using System.Threading.Tasks;
namespace WebBanHang.Repository.IRepository {
    public interface IProductVariantRepository : IRepository<ProductVariant>
    {
        Task<bool> TryDecreaseStockAsync(long variantId, int quantity);
        Task IncreaseStockAsync(long variantId, int quantity);
    }
}
