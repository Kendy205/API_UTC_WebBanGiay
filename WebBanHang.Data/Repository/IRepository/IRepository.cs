using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;

namespace WebBanHang.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, int? pageSize = null, int? pageNumber = null);
        //Task<IEnumerable<T>> GetPageAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, int pageSize = 10, int pageNumber = 1);
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, string? includeProperties = null);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
