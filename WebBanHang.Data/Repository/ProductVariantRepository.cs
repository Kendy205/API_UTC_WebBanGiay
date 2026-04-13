using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Model;
using WebBanHang.Repository.IRepository;

namespace WebBanHang.Repository
{
    public class ProductVariantRepository : Repository<ProductVariant>, IProductVariantRepository
    {
        private readonly AppDbContext _context;

        public ProductVariantRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> TryDecreaseStockAsync(long variantId, int quantity)
        {
            if (quantity <= 0) return false;

            var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE product_variants
                SET stock_quantity = stock_quantity - {quantity}
                WHERE variant_id = {variantId} AND stock_quantity >= {quantity}");

            return affectedRows > 0;
        }

        public async Task IncreaseStockAsync(long variantId, int quantity)
        {
            if (quantity <= 0) return;

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE product_variants
                SET stock_quantity = stock_quantity + {quantity}
                WHERE variant_id = {variantId}");
        }
    }
}
